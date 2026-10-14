namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool rollups by code.</summary>
public interface IPoolRollupStore
{
    PoolRollup? Find(string code);
    void Save(PoolRollup item);
    bool Remove(string code);
    List<PoolRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolRollupStore(Dictionary<string, PoolRollup> items, List<string> order) : IPoolRollupStore
{
    public Dictionary<string, PoolRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolRollup item)
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

    public List<PoolRollup> List()
    {
        var outValue = new List<PoolRollup>();
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
public sealed class CachedPoolRollupStore(MemoryPoolRollupStore inner, PoolRollupCache cache, int hits, int misses) : IPoolRollupStore
{
    public MemoryPoolRollupStore Inner { get; set; } = inner;
    public PoolRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolRollup? Find(string code)
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

    public void Save(PoolRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool rollups. Empty fields match all.</summary>
public sealed record PoolRollupQuery(string Country, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PoolRollup item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolRollup> Apply(List<PoolRollup> items)
    {
        var matched = Collections.Filter(items, (PoolRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool rollup store.</summary>
public static class PoolRollupStore
{
    public static IPoolRollupStore NewMemoryPoolRollupStore()
    {
        return new MemoryPoolRollupStore(new Dictionary<string, PoolRollup>(), new List<string>());
    }

    public static IPoolRollupStore NewCachedPoolRollupStore(MemoryPoolRollupStore inner, int ttl, IClock clock)
    {
        return new CachedPoolRollupStore(inner, PoolRollupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PoolRollup> FindPoolRollups(IPoolRollupStore store, PoolRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
