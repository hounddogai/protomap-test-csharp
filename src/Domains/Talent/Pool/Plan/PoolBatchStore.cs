namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool batches by code.</summary>
public interface IPoolBatchStore
{
    PoolBatch? Find(string code);
    void Save(PoolBatch item);
    bool Remove(string code);
    List<PoolBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolBatchStore(Dictionary<string, PoolBatch> items, List<string> order) : IPoolBatchStore
{
    public Dictionary<string, PoolBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolBatch item)
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

    public List<PoolBatch> List()
    {
        var outValue = new List<PoolBatch>();
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
public sealed class CachedPoolBatchStore(MemoryPoolBatchStore inner, PoolBatchCache cache, int hits, int misses) : IPoolBatchStore
{
    public MemoryPoolBatchStore Inner { get; set; } = inner;
    public PoolBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolBatch? Find(string code)
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

    public void Save(PoolBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool batches. Empty fields match all.</summary>
public sealed record PoolBatchQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PoolBatch item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolBatch> Apply(List<PoolBatch> items)
    {
        var matched = Collections.Filter(items, (PoolBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool batch store.</summary>
public static class PoolBatchStore
{
    public static IPoolBatchStore NewMemoryPoolBatchStore()
    {
        return new MemoryPoolBatchStore(new Dictionary<string, PoolBatch>(), new List<string>());
    }

    public static IPoolBatchStore NewCachedPoolBatchStore(MemoryPoolBatchStore inner, int ttl, IClock clock)
    {
        return new CachedPoolBatchStore(inner, PoolBatchCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PoolBatch> FindPoolBatches(IPoolBatchStore store, PoolBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
