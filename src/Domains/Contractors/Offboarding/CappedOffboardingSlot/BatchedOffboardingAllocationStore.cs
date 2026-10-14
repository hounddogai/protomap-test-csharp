namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched offboarding allocations by code.</summary>
public interface IBatchedOffboardingAllocationStore
{
    BatchedOffboardingAllocation? Find(string code);
    void Save(BatchedOffboardingAllocation item);
    bool Remove(string code);
    List<BatchedOffboardingAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedOffboardingAllocationStore(Dictionary<string, BatchedOffboardingAllocation> items, List<string> order) : IBatchedOffboardingAllocationStore
{
    public Dictionary<string, BatchedOffboardingAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedOffboardingAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedOffboardingAllocation item)
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

    public List<BatchedOffboardingAllocation> List()
    {
        var outValue = new List<BatchedOffboardingAllocation>();
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
public sealed class CachedBatchedOffboardingAllocationStore(MemoryBatchedOffboardingAllocationStore inner, BatchedOffboardingAllocationCache cache, int hits, int misses) : IBatchedOffboardingAllocationStore
{
    public MemoryBatchedOffboardingAllocationStore Inner { get; set; } = inner;
    public BatchedOffboardingAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedOffboardingAllocation? Find(string code)
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

    public void Save(BatchedOffboardingAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedOffboardingAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched offboarding allocations. Empty fields match all.</summary>
public sealed record BatchedOffboardingAllocationQuery(string Label, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedOffboardingAllocation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedOffboardingAllocation> Apply(List<BatchedOffboardingAllocation> items)
    {
        var matched = Collections.Filter(items, (BatchedOffboardingAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched offboarding allocation store.</summary>
public static class BatchedOffboardingAllocationStore
{
    public static IBatchedOffboardingAllocationStore NewMemoryBatchedOffboardingAllocationStore()
    {
        return new MemoryBatchedOffboardingAllocationStore(
            new Dictionary<string, BatchedOffboardingAllocation>(),
            new List<string>());
    }

    public static IBatchedOffboardingAllocationStore NewCachedBatchedOffboardingAllocationStore(MemoryBatchedOffboardingAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedOffboardingAllocationStore(
            inner,
            BatchedOffboardingAllocationCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<BatchedOffboardingAllocation> FindBatchedOffboardingAllocations(IBatchedOffboardingAllocationStore store, BatchedOffboardingAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
