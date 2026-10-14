namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan allocations by code.</summary>
public interface IFloorPlanAllocationStore
{
    FloorPlanAllocation? Find(string code);
    void Save(FloorPlanAllocation item);
    bool Remove(string code);
    List<FloorPlanAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanAllocationStore(Dictionary<string, FloorPlanAllocation> items, List<string> order) : IFloorPlanAllocationStore
{
    public Dictionary<string, FloorPlanAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanAllocation item)
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

    public List<FloorPlanAllocation> List()
    {
        var outValue = new List<FloorPlanAllocation>();
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
public sealed class CachedFloorPlanAllocationStore(MemoryFloorPlanAllocationStore inner, FloorPlanAllocationCache cache, int hits, int misses) : IFloorPlanAllocationStore
{
    public MemoryFloorPlanAllocationStore Inner { get; set; } = inner;
    public FloorPlanAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanAllocation? Find(string code)
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

    public void Save(FloorPlanAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan allocations. Empty fields match all.</summary>
public sealed record FloorPlanAllocationQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanAllocation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanAllocation> Apply(List<FloorPlanAllocation> items)
    {
        var matched = Collections.Filter(items, (FloorPlanAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan allocation store.</summary>
public static class FloorPlanAllocationStore
{
    public static IFloorPlanAllocationStore NewMemoryFloorPlanAllocationStore()
    {
        return new MemoryFloorPlanAllocationStore(new Dictionary<string, FloorPlanAllocation>(), new List<string>());
    }

    public static IFloorPlanAllocationStore NewCachedFloorPlanAllocationStore(MemoryFloorPlanAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanAllocationStore(inner, FloorPlanAllocationCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FloorPlanAllocation> FindFloorPlanAllocations(IFloorPlanAllocationStore store, FloorPlanAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
