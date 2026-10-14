namespace Acme.Workforce.Workplace.Booking.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual booking allocations by code.</summary>
public interface IManualBookingAllocationStore
{
    ManualBookingAllocation? Find(string code);
    void Save(ManualBookingAllocation item);
    bool Remove(string code);
    List<ManualBookingAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBookingAllocationStore(Dictionary<string, ManualBookingAllocation> items, List<string> order) : IManualBookingAllocationStore
{
    public Dictionary<string, ManualBookingAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBookingAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBookingAllocation item)
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

    public List<ManualBookingAllocation> List()
    {
        var outValue = new List<ManualBookingAllocation>();
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
public sealed class CachedManualBookingAllocationStore(MemoryManualBookingAllocationStore inner, ManualBookingAllocationCache cache, int hits, int misses) : IManualBookingAllocationStore
{
    public MemoryManualBookingAllocationStore Inner { get; set; } = inner;
    public ManualBookingAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBookingAllocation? Find(string code)
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

    public void Save(ManualBookingAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBookingAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual booking allocations. Empty fields match all.</summary>
public sealed record ManualBookingAllocationQuery(string Category, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBookingAllocation item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<ManualBookingAllocation> Apply(List<ManualBookingAllocation> items)
    {
        var matched = Collections.Filter(items, (ManualBookingAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual booking allocation store.</summary>
public static class ManualBookingAllocationStore
{
    public static IManualBookingAllocationStore NewMemoryManualBookingAllocationStore()
    {
        return new MemoryManualBookingAllocationStore(
            new Dictionary<string, ManualBookingAllocation>(),
            new List<string>());
    }

    public static IManualBookingAllocationStore NewCachedManualBookingAllocationStore(MemoryManualBookingAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedManualBookingAllocationStore(
            inner,
            ManualBookingAllocationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualBookingAllocation> FindManualBookingAllocations(IManualBookingAllocationStore store, ManualBookingAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
