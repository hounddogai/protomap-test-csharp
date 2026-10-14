namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched access badge allocations by code.</summary>
public interface IBatchedAccessBadgeAllocationStore
{
    BatchedAccessBadgeAllocation? Find(string code);
    void Save(BatchedAccessBadgeAllocation item);
    bool Remove(string code);
    List<BatchedAccessBadgeAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAccessBadgeAllocationStore(Dictionary<string, BatchedAccessBadgeAllocation> items, List<string> order) : IBatchedAccessBadgeAllocationStore
{
    public Dictionary<string, BatchedAccessBadgeAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAccessBadgeAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAccessBadgeAllocation item)
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

    public List<BatchedAccessBadgeAllocation> List()
    {
        var outValue = new List<BatchedAccessBadgeAllocation>();
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
public sealed class CachedBatchedAccessBadgeAllocationStore(MemoryBatchedAccessBadgeAllocationStore inner, BatchedAccessBadgeAllocationCache cache, int hits, int misses) : IBatchedAccessBadgeAllocationStore
{
    public MemoryBatchedAccessBadgeAllocationStore Inner { get; set; } = inner;
    public BatchedAccessBadgeAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAccessBadgeAllocation? Find(string code)
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

    public void Save(BatchedAccessBadgeAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAccessBadgeAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched access badge allocations. Empty fields match all.</summary>
public sealed record BatchedAccessBadgeAllocationQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAccessBadgeAllocation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<BatchedAccessBadgeAllocation> Apply(List<BatchedAccessBadgeAllocation> items)
    {
        var matched = Collections.Filter(items, (BatchedAccessBadgeAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched access badge allocation store.</summary>
public static class BatchedAccessBadgeAllocationStore
{
    public static IBatchedAccessBadgeAllocationStore NewMemoryBatchedAccessBadgeAllocationStore()
    {
        return new MemoryBatchedAccessBadgeAllocationStore(
            new Dictionary<string, BatchedAccessBadgeAllocation>(),
            new List<string>());
    }

    public static IBatchedAccessBadgeAllocationStore NewCachedBatchedAccessBadgeAllocationStore(MemoryBatchedAccessBadgeAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAccessBadgeAllocationStore(
            inner,
            BatchedAccessBadgeAllocationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedAccessBadgeAllocation> FindBatchedAccessBadgeAllocations(IBatchedAccessBadgeAllocationStore store, BatchedAccessBadgeAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
