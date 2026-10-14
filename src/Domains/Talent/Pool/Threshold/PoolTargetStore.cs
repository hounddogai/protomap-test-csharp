namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool targets by code.</summary>
public interface IPoolTargetStore
{
    PoolTarget? Find(string code);
    void Save(PoolTarget item);
    bool Remove(string code);
    List<PoolTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolTargetStore(Dictionary<string, PoolTarget> items, List<string> order) : IPoolTargetStore
{
    public Dictionary<string, PoolTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolTarget item)
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

    public List<PoolTarget> List()
    {
        var outValue = new List<PoolTarget>();
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
public sealed class CachedPoolTargetStore(MemoryPoolTargetStore inner, PoolTargetCache cache, int hits, int misses) : IPoolTargetStore
{
    public MemoryPoolTargetStore Inner { get; set; } = inner;
    public PoolTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolTarget? Find(string code)
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

    public void Save(PoolTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool targets. Empty fields match all.</summary>
public sealed record PoolTargetQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(PoolTarget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolTarget> Apply(List<PoolTarget> items)
    {
        var matched = Collections.Filter(items, (PoolTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool target store.</summary>
public static class PoolTargetStore
{
    public static IPoolTargetStore NewMemoryPoolTargetStore()
    {
        return new MemoryPoolTargetStore(new Dictionary<string, PoolTarget>(), new List<string>());
    }

    public static IPoolTargetStore NewCachedPoolTargetStore(MemoryPoolTargetStore inner, int ttl, IClock clock)
    {
        return new CachedPoolTargetStore(inner, PoolTargetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PoolTarget> FindPoolTargets(IPoolTargetStore store, PoolTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
