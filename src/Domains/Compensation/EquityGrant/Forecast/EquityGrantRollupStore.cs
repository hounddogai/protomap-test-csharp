namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant rollups by code.</summary>
public interface IEquityGrantRollupStore
{
    EquityGrantRollup? Find(string code);
    void Save(EquityGrantRollup item);
    bool Remove(string code);
    List<EquityGrantRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantRollupStore(Dictionary<string, EquityGrantRollup> items, List<string> order) : IEquityGrantRollupStore
{
    public Dictionary<string, EquityGrantRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantRollup item)
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

    public List<EquityGrantRollup> List()
    {
        var outValue = new List<EquityGrantRollup>();
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
public sealed class CachedEquityGrantRollupStore(MemoryEquityGrantRollupStore inner, EquityGrantRollupCache cache, int hits, int misses) : IEquityGrantRollupStore
{
    public MemoryEquityGrantRollupStore Inner { get; set; } = inner;
    public EquityGrantRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantRollup? Find(string code)
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

    public void Save(EquityGrantRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant rollups. Empty fields match all.</summary>
public sealed record EquityGrantRollupQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantRollup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantRollup> Apply(List<EquityGrantRollup> items)
    {
        var matched = Collections.Filter(items, (EquityGrantRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant rollup store.</summary>
public static class EquityGrantRollupStore
{
    public static IEquityGrantRollupStore NewMemoryEquityGrantRollupStore()
    {
        return new MemoryEquityGrantRollupStore(new Dictionary<string, EquityGrantRollup>(), new List<string>());
    }

    public static IEquityGrantRollupStore NewCachedEquityGrantRollupStore(MemoryEquityGrantRollupStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantRollupStore(inner, EquityGrantRollupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<EquityGrantRollup> FindEquityGrantRollups(IEquityGrantRollupStore store, EquityGrantRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
