namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled adjustment allocations by code.</summary>
public interface IScheduledAdjustmentAllocationStore
{
    ScheduledAdjustmentAllocation? Find(string code);
    void Save(ScheduledAdjustmentAllocation item);
    bool Remove(string code);
    List<ScheduledAdjustmentAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAdjustmentAllocationStore(Dictionary<string, ScheduledAdjustmentAllocation> items, List<string> order) : IScheduledAdjustmentAllocationStore
{
    public Dictionary<string, ScheduledAdjustmentAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAdjustmentAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAdjustmentAllocation item)
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

    public List<ScheduledAdjustmentAllocation> List()
    {
        var outValue = new List<ScheduledAdjustmentAllocation>();
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
public sealed class CachedScheduledAdjustmentAllocationStore(MemoryScheduledAdjustmentAllocationStore inner, ScheduledAdjustmentAllocationCache cache, int hits, int misses) : IScheduledAdjustmentAllocationStore
{
    public MemoryScheduledAdjustmentAllocationStore Inner { get; set; } = inner;
    public ScheduledAdjustmentAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAdjustmentAllocation? Find(string code)
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

    public void Save(ScheduledAdjustmentAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAdjustmentAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled adjustment allocations. Empty fields match all.</summary>
public sealed record ScheduledAdjustmentAllocationQuery(string Region, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAdjustmentAllocation item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAdjustmentAllocation> Apply(List<ScheduledAdjustmentAllocation> items)
    {
        var matched = Collections.Filter(items, (ScheduledAdjustmentAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled adjustment allocation store.</summary>
public static class ScheduledAdjustmentAllocationStore
{
    public static IScheduledAdjustmentAllocationStore NewMemoryScheduledAdjustmentAllocationStore()
    {
        return new MemoryScheduledAdjustmentAllocationStore(
            new Dictionary<string, ScheduledAdjustmentAllocation>(),
            new List<string>());
    }

    public static IScheduledAdjustmentAllocationStore NewCachedScheduledAdjustmentAllocationStore(MemoryScheduledAdjustmentAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAdjustmentAllocationStore(
            inner,
            ScheduledAdjustmentAllocationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAdjustmentAllocation> FindScheduledAdjustmentAllocations(IScheduledAdjustmentAllocationStore store, ScheduledAdjustmentAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
