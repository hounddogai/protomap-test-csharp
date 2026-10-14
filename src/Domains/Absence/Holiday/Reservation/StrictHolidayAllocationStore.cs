namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict holiday allocations by code.</summary>
public interface IStrictHolidayAllocationStore
{
    StrictHolidayAllocation? Find(string code);
    void Save(StrictHolidayAllocation item);
    bool Remove(string code);
    List<StrictHolidayAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictHolidayAllocationStore(Dictionary<string, StrictHolidayAllocation> items, List<string> order) : IStrictHolidayAllocationStore
{
    public Dictionary<string, StrictHolidayAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictHolidayAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictHolidayAllocation item)
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

    public List<StrictHolidayAllocation> List()
    {
        var outValue = new List<StrictHolidayAllocation>();
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
public sealed class CachedStrictHolidayAllocationStore(MemoryStrictHolidayAllocationStore inner, StrictHolidayAllocationCache cache, int hits, int misses) : IStrictHolidayAllocationStore
{
    public MemoryStrictHolidayAllocationStore Inner { get; set; } = inner;
    public StrictHolidayAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictHolidayAllocation? Find(string code)
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

    public void Save(StrictHolidayAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictHolidayAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict holiday allocations. Empty fields match all.</summary>
public sealed record StrictHolidayAllocationQuery(string Category, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(StrictHolidayAllocation item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictHolidayAllocation> Apply(List<StrictHolidayAllocation> items)
    {
        var matched = Collections.Filter(items, (StrictHolidayAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict holiday allocation store.</summary>
public static class StrictHolidayAllocationStore
{
    public static IStrictHolidayAllocationStore NewMemoryStrictHolidayAllocationStore()
    {
        return new MemoryStrictHolidayAllocationStore(
            new Dictionary<string, StrictHolidayAllocation>(),
            new List<string>());
    }

    public static IStrictHolidayAllocationStore NewCachedStrictHolidayAllocationStore(MemoryStrictHolidayAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedStrictHolidayAllocationStore(
            inner,
            StrictHolidayAllocationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StrictHolidayAllocation> FindStrictHolidayAllocations(IStrictHolidayAllocationStore store, StrictHolidayAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
