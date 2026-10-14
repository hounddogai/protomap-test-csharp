namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan cycles by code.</summary>
public interface IFloorPlanCycleStore
{
    FloorPlanCycle? Find(string code);
    void Save(FloorPlanCycle item);
    bool Remove(string code);
    List<FloorPlanCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanCycleStore(Dictionary<string, FloorPlanCycle> items, List<string> order) : IFloorPlanCycleStore
{
    public Dictionary<string, FloorPlanCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanCycle item)
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

    public List<FloorPlanCycle> List()
    {
        var outValue = new List<FloorPlanCycle>();
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
public sealed class CachedFloorPlanCycleStore(MemoryFloorPlanCycleStore inner, FloorPlanCycleCache cache, int hits, int misses) : IFloorPlanCycleStore
{
    public MemoryFloorPlanCycleStore Inner { get; set; } = inner;
    public FloorPlanCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanCycle? Find(string code)
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

    public void Save(FloorPlanCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan cycles. Empty fields match all.</summary>
public sealed record FloorPlanCycleQuery(string Unit, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanCycle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<FloorPlanCycle> Apply(List<FloorPlanCycle> items)
    {
        var matched = Collections.Filter(items, (FloorPlanCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan cycle store.</summary>
public static class FloorPlanCycleStore
{
    public static IFloorPlanCycleStore NewMemoryFloorPlanCycleStore()
    {
        return new MemoryFloorPlanCycleStore(new Dictionary<string, FloorPlanCycle>(), new List<string>());
    }

    public static IFloorPlanCycleStore NewCachedFloorPlanCycleStore(MemoryFloorPlanCycleStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanCycleStore(inner, FloorPlanCycleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FloorPlanCycle> FindFloorPlanCycles(IFloorPlanCycleStore store, FloorPlanCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
