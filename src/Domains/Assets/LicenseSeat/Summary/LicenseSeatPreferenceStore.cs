namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat preferences by code.</summary>
public interface ILicenseSeatPreferenceStore
{
    LicenseSeatPreference? Find(string code);
    void Save(LicenseSeatPreference item);
    bool Remove(string code);
    List<LicenseSeatPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatPreferenceStore(Dictionary<string, LicenseSeatPreference> items, List<string> order) : ILicenseSeatPreferenceStore
{
    public Dictionary<string, LicenseSeatPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatPreference item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<LicenseSeatPreference> List()
    {
        var outValue = new List<LicenseSeatPreference>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedLicenseSeatPreferenceStore(MemoryLicenseSeatPreferenceStore inner, LicenseSeatPreferenceCache cache, int hits, int misses) : ILicenseSeatPreferenceStore
{
    public MemoryLicenseSeatPreferenceStore Inner { get; set; } = inner;
    public LicenseSeatPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatPreference? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(LicenseSeatPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat preferences. Empty fields match all.</summary>
public sealed record LicenseSeatPreferenceQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatPreference item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatPreference> Apply(List<LicenseSeatPreference> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat preference store.</summary>
public static class LicenseSeatPreferenceStore
{
    public static ILicenseSeatPreferenceStore NewMemoryLicenseSeatPreferenceStore()
    {
        return new MemoryLicenseSeatPreferenceStore(
            new Dictionary<string, LicenseSeatPreference>(),
            new List<string>());
    }

    public static ILicenseSeatPreferenceStore NewCachedLicenseSeatPreferenceStore(MemoryLicenseSeatPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatPreferenceStore(inner, LicenseSeatPreferenceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatPreference> FindLicenseSeatPreferences(ILicenseSeatPreferenceStore store, LicenseSeatPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
