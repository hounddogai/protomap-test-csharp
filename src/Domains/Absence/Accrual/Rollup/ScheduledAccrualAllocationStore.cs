namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled accrual allocations by code.</summary>
public interface IScheduledAccrualAllocationStore
{
    ScheduledAccrualAllocation? Find(string code);
    void Save(ScheduledAccrualAllocation item);
    bool Remove(string code);
    List<ScheduledAccrualAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAccrualAllocationStore(Dictionary<string, ScheduledAccrualAllocation> items, List<string> order) : IScheduledAccrualAllocationStore
{
    public Dictionary<string, ScheduledAccrualAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAccrualAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAccrualAllocation item)
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

    public List<ScheduledAccrualAllocation> List()
    {
        var outValue = new List<ScheduledAccrualAllocation>();
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
public sealed class CachedScheduledAccrualAllocationStore(MemoryScheduledAccrualAllocationStore inner, ScheduledAccrualAllocationCache cache, int hits, int misses) : IScheduledAccrualAllocationStore
{
    public MemoryScheduledAccrualAllocationStore Inner { get; set; } = inner;
    public ScheduledAccrualAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAccrualAllocation? Find(string code)
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

    public void Save(ScheduledAccrualAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAccrualAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled accrual allocations. Empty fields match all.</summary>
public sealed record ScheduledAccrualAllocationQuery(string Locale, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAccrualAllocation item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAccrualAllocation> Apply(List<ScheduledAccrualAllocation> items)
    {
        var matched = Collections.Filter(items, (ScheduledAccrualAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled accrual allocation store.</summary>
public static class ScheduledAccrualAllocationStore
{
    public static IScheduledAccrualAllocationStore NewMemoryScheduledAccrualAllocationStore()
    {
        return new MemoryScheduledAccrualAllocationStore(
            new Dictionary<string, ScheduledAccrualAllocation>(),
            new List<string>());
    }

    public static IScheduledAccrualAllocationStore NewCachedScheduledAccrualAllocationStore(MemoryScheduledAccrualAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAccrualAllocationStore(
            inner,
            ScheduledAccrualAllocationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAccrualAllocation> FindScheduledAccrualAllocations(IScheduledAccrualAllocationStore store, ScheduledAccrualAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
