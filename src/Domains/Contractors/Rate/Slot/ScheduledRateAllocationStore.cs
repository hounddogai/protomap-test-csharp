namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled rate allocations by code.</summary>
public interface IScheduledRateAllocationStore
{
    ScheduledRateAllocation? Find(string code);
    void Save(ScheduledRateAllocation item);
    bool Remove(string code);
    List<ScheduledRateAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledRateAllocationStore(Dictionary<string, ScheduledRateAllocation> items, List<string> order) : IScheduledRateAllocationStore
{
    public Dictionary<string, ScheduledRateAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledRateAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledRateAllocation item)
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

    public List<ScheduledRateAllocation> List()
    {
        var outValue = new List<ScheduledRateAllocation>();
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
public sealed class CachedScheduledRateAllocationStore(MemoryScheduledRateAllocationStore inner, ScheduledRateAllocationCache cache, int hits, int misses) : IScheduledRateAllocationStore
{
    public MemoryScheduledRateAllocationStore Inner { get; set; } = inner;
    public ScheduledRateAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledRateAllocation? Find(string code)
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

    public void Save(ScheduledRateAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledRateAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled rate allocations. Empty fields match all.</summary>
public sealed record ScheduledRateAllocationQuery(string Department, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledRateAllocation item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledRateAllocation> Apply(List<ScheduledRateAllocation> items)
    {
        var matched = Collections.Filter(items, (ScheduledRateAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled rate allocation store.</summary>
public static class ScheduledRateAllocationStore
{
    public static IScheduledRateAllocationStore NewMemoryScheduledRateAllocationStore()
    {
        return new MemoryScheduledRateAllocationStore(
            new Dictionary<string, ScheduledRateAllocation>(),
            new List<string>());
    }

    public static IScheduledRateAllocationStore NewCachedScheduledRateAllocationStore(MemoryScheduledRateAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledRateAllocationStore(
            inner,
            ScheduledRateAllocationCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledRateAllocation> FindScheduledRateAllocations(IScheduledRateAllocationStore store, ScheduledRateAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
