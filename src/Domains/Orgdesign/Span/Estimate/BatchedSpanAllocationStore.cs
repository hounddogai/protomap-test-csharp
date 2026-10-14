namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched span allocations by code.</summary>
public interface IBatchedSpanAllocationStore
{
    BatchedSpanAllocation? Find(string code);
    void Save(BatchedSpanAllocation item);
    bool Remove(string code);
    List<BatchedSpanAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedSpanAllocationStore(Dictionary<string, BatchedSpanAllocation> items, List<string> order) : IBatchedSpanAllocationStore
{
    public Dictionary<string, BatchedSpanAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedSpanAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedSpanAllocation item)
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

    public List<BatchedSpanAllocation> List()
    {
        var outValue = new List<BatchedSpanAllocation>();
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
public sealed class CachedBatchedSpanAllocationStore(MemoryBatchedSpanAllocationStore inner, BatchedSpanAllocationCache cache, int hits, int misses) : IBatchedSpanAllocationStore
{
    public MemoryBatchedSpanAllocationStore Inner { get; set; } = inner;
    public BatchedSpanAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedSpanAllocation? Find(string code)
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

    public void Save(BatchedSpanAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedSpanAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched span allocations. Empty fields match all.</summary>
public sealed record BatchedSpanAllocationQuery(string Unit, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedSpanAllocation item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedSpanAllocation> Apply(List<BatchedSpanAllocation> items)
    {
        var matched = Collections.Filter(items, (BatchedSpanAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched span allocation store.</summary>
public static class BatchedSpanAllocationStore
{
    public static IBatchedSpanAllocationStore NewMemoryBatchedSpanAllocationStore()
    {
        return new MemoryBatchedSpanAllocationStore(
            new Dictionary<string, BatchedSpanAllocation>(),
            new List<string>());
    }

    public static IBatchedSpanAllocationStore NewCachedBatchedSpanAllocationStore(MemoryBatchedSpanAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedSpanAllocationStore(inner, BatchedSpanAllocationCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BatchedSpanAllocation> FindBatchedSpanAllocations(IBatchedSpanAllocationStore store, BatchedSpanAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
