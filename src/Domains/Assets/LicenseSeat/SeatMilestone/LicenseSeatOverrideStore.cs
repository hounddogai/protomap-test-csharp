namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat overrides by code.</summary>
public interface ILicenseSeatOverrideStore
{
    LicenseSeatOverride? Find(string code);
    void Save(LicenseSeatOverride item);
    bool Remove(string code);
    List<LicenseSeatOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatOverrideStore(Dictionary<string, LicenseSeatOverride> items, List<string> order) : ILicenseSeatOverrideStore
{
    public Dictionary<string, LicenseSeatOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatOverride item)
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

    public List<LicenseSeatOverride> List()
    {
        var outValue = new List<LicenseSeatOverride>();
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
public sealed class CachedLicenseSeatOverrideStore(MemoryLicenseSeatOverrideStore inner, LicenseSeatOverrideCache cache, int hits, int misses) : ILicenseSeatOverrideStore
{
    public MemoryLicenseSeatOverrideStore Inner { get; set; } = inner;
    public LicenseSeatOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatOverride? Find(string code)
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

    public void Save(LicenseSeatOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat overrides. Empty fields match all.</summary>
public sealed record LicenseSeatOverrideQuery(string Category, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatOverride item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatOverride> Apply(List<LicenseSeatOverride> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat override store.</summary>
public static class LicenseSeatOverrideStore
{
    public static ILicenseSeatOverrideStore NewMemoryLicenseSeatOverrideStore()
    {
        return new MemoryLicenseSeatOverrideStore(new Dictionary<string, LicenseSeatOverride>(), new List<string>());
    }

    public static ILicenseSeatOverrideStore NewCachedLicenseSeatOverrideStore(MemoryLicenseSeatOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatOverrideStore(inner, LicenseSeatOverrideCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatOverride> FindLicenseSeatOverrides(ILicenseSeatOverrideStore store, LicenseSeatOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
