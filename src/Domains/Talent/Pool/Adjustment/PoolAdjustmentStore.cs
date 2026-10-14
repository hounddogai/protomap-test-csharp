namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool adjustments by code.</summary>
public interface IPoolAdjustmentStore
{
    PoolAdjustment? Find(string code);
    void Save(PoolAdjustment item);
    bool Remove(string code);
    List<PoolAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolAdjustmentStore(Dictionary<string, PoolAdjustment> items, List<string> order) : IPoolAdjustmentStore
{
    public Dictionary<string, PoolAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolAdjustment item)
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

    public List<PoolAdjustment> List()
    {
        var outValue = new List<PoolAdjustment>();
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
public sealed class CachedPoolAdjustmentStore(MemoryPoolAdjustmentStore inner, PoolAdjustmentCache cache, int hits, int misses) : IPoolAdjustmentStore
{
    public MemoryPoolAdjustmentStore Inner { get; set; } = inner;
    public PoolAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolAdjustment? Find(string code)
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

    public void Save(PoolAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool adjustments. Empty fields match all.</summary>
public sealed record PoolAdjustmentQuery(string Source, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PoolAdjustment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolAdjustment> Apply(List<PoolAdjustment> items)
    {
        var matched = Collections.Filter(items, (PoolAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool adjustment store.</summary>
public static class PoolAdjustmentStore
{
    public static IPoolAdjustmentStore NewMemoryPoolAdjustmentStore()
    {
        return new MemoryPoolAdjustmentStore(new Dictionary<string, PoolAdjustment>(), new List<string>());
    }

    public static IPoolAdjustmentStore NewCachedPoolAdjustmentStore(MemoryPoolAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPoolAdjustmentStore(inner, PoolAdjustmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PoolAdjustment> FindPoolAdjustments(IPoolAdjustmentStore store, PoolAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
