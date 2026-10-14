namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy rate preferences by code.</summary>
public interface ILegacyRatePreferenceStore
{
    LegacyRatePreference? Find(string code);
    void Save(LegacyRatePreference item);
    bool Remove(string code);
    List<LegacyRatePreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyRatePreferenceStore(Dictionary<string, LegacyRatePreference> items, List<string> order) : ILegacyRatePreferenceStore
{
    public Dictionary<string, LegacyRatePreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyRatePreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyRatePreference item)
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

    public List<LegacyRatePreference> List()
    {
        var outValue = new List<LegacyRatePreference>();
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
public sealed class CachedLegacyRatePreferenceStore(MemoryLegacyRatePreferenceStore inner, LegacyRatePreferenceCache cache, int hits, int misses) : ILegacyRatePreferenceStore
{
    public MemoryLegacyRatePreferenceStore Inner { get; set; } = inner;
    public LegacyRatePreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyRatePreference? Find(string code)
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

    public void Save(LegacyRatePreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyRatePreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy rate preferences. Empty fields match all.</summary>
public sealed record LegacyRatePreferenceQuery(string Title, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyRatePreference item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyRatePreference> Apply(List<LegacyRatePreference> items)
    {
        var matched = Collections.Filter(items, (LegacyRatePreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy rate preference store.</summary>
public static class LegacyRatePreferenceStore
{
    public static ILegacyRatePreferenceStore NewMemoryLegacyRatePreferenceStore()
    {
        return new MemoryLegacyRatePreferenceStore(
            new Dictionary<string, LegacyRatePreference>(),
            new List<string>());
    }

    public static ILegacyRatePreferenceStore NewCachedLegacyRatePreferenceStore(MemoryLegacyRatePreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyRatePreferenceStore(inner, LegacyRatePreferenceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LegacyRatePreference> FindLegacyRatePreferences(ILegacyRatePreferenceStore store, LegacyRatePreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
