namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional merit cycle bundles by code.</summary>
public interface IRegionalMeritCycleBundleStore
{
    RegionalMeritCycleBundle? Find(string code);
    void Save(RegionalMeritCycleBundle item);
    bool Remove(string code);
    List<RegionalMeritCycleBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalMeritCycleBundleStore(Dictionary<string, RegionalMeritCycleBundle> items, List<string> order) : IRegionalMeritCycleBundleStore
{
    public Dictionary<string, RegionalMeritCycleBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalMeritCycleBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalMeritCycleBundle item)
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

    public List<RegionalMeritCycleBundle> List()
    {
        var outValue = new List<RegionalMeritCycleBundle>();
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
public sealed class CachedRegionalMeritCycleBundleStore(MemoryRegionalMeritCycleBundleStore inner, RegionalMeritCycleBundleCache cache, int hits, int misses) : IRegionalMeritCycleBundleStore
{
    public MemoryRegionalMeritCycleBundleStore Inner { get; set; } = inner;
    public RegionalMeritCycleBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalMeritCycleBundle? Find(string code)
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

    public void Save(RegionalMeritCycleBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalMeritCycleBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional merit cycle bundles. Empty fields match all.</summary>
public sealed record RegionalMeritCycleBundleQuery(string Team, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalMeritCycleBundle item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalMeritCycleBundle> Apply(List<RegionalMeritCycleBundle> items)
    {
        var matched = Collections.Filter(items, (RegionalMeritCycleBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional merit cycle bundle store.</summary>
public static class RegionalMeritCycleBundleStore
{
    public static IRegionalMeritCycleBundleStore NewMemoryRegionalMeritCycleBundleStore()
    {
        return new MemoryRegionalMeritCycleBundleStore(
            new Dictionary<string, RegionalMeritCycleBundle>(),
            new List<string>());
    }

    public static IRegionalMeritCycleBundleStore NewCachedRegionalMeritCycleBundleStore(MemoryRegionalMeritCycleBundleStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalMeritCycleBundleStore(
            inner,
            RegionalMeritCycleBundleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalMeritCycleBundle> FindRegionalMeritCycleBundles(IRegionalMeritCycleBundleStore store, RegionalMeritCycleBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
