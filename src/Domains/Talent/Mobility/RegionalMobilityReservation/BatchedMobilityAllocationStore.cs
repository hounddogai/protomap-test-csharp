namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched mobility allocations by code.</summary>
public interface IBatchedMobilityAllocationStore
{
    BatchedMobilityAllocation? Find(string code);
    void Save(BatchedMobilityAllocation item);
    bool Remove(string code);
    List<BatchedMobilityAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedMobilityAllocationStore(Dictionary<string, BatchedMobilityAllocation> items, List<string> order) : IBatchedMobilityAllocationStore
{
    public Dictionary<string, BatchedMobilityAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedMobilityAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedMobilityAllocation item)
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

    public List<BatchedMobilityAllocation> List()
    {
        var outValue = new List<BatchedMobilityAllocation>();
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
public sealed class CachedBatchedMobilityAllocationStore(MemoryBatchedMobilityAllocationStore inner, BatchedMobilityAllocationCache cache, int hits, int misses) : IBatchedMobilityAllocationStore
{
    public MemoryBatchedMobilityAllocationStore Inner { get; set; } = inner;
    public BatchedMobilityAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedMobilityAllocation? Find(string code)
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

    public void Save(BatchedMobilityAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedMobilityAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched mobility allocations. Empty fields match all.</summary>
public sealed record BatchedMobilityAllocationQuery(string Segment, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedMobilityAllocation item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedMobilityAllocation> Apply(List<BatchedMobilityAllocation> items)
    {
        var matched = Collections.Filter(items, (BatchedMobilityAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched mobility allocation store.</summary>
public static class BatchedMobilityAllocationStore
{
    public static IBatchedMobilityAllocationStore NewMemoryBatchedMobilityAllocationStore()
    {
        return new MemoryBatchedMobilityAllocationStore(
            new Dictionary<string, BatchedMobilityAllocation>(),
            new List<string>());
    }

    public static IBatchedMobilityAllocationStore NewCachedBatchedMobilityAllocationStore(MemoryBatchedMobilityAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedMobilityAllocationStore(
            inner,
            BatchedMobilityAllocationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedMobilityAllocation> FindBatchedMobilityAllocations(IBatchedMobilityAllocationStore store, BatchedMobilityAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
