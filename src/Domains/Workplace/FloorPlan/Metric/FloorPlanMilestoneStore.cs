namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan milestones by code.</summary>
public interface IFloorPlanMilestoneStore
{
    FloorPlanMilestone? Find(string code);
    void Save(FloorPlanMilestone item);
    bool Remove(string code);
    List<FloorPlanMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanMilestoneStore(Dictionary<string, FloorPlanMilestone> items, List<string> order) : IFloorPlanMilestoneStore
{
    public Dictionary<string, FloorPlanMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanMilestone item)
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

    public List<FloorPlanMilestone> List()
    {
        var outValue = new List<FloorPlanMilestone>();
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
public sealed class CachedFloorPlanMilestoneStore(MemoryFloorPlanMilestoneStore inner, FloorPlanMilestoneCache cache, int hits, int misses) : IFloorPlanMilestoneStore
{
    public MemoryFloorPlanMilestoneStore Inner { get; set; } = inner;
    public FloorPlanMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanMilestone? Find(string code)
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

    public void Save(FloorPlanMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan milestones. Empty fields match all.</summary>
public sealed record FloorPlanMilestoneQuery(string Channel, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanMilestone item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<FloorPlanMilestone> Apply(List<FloorPlanMilestone> items)
    {
        var matched = Collections.Filter(items, (FloorPlanMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan milestone store.</summary>
public static class FloorPlanMilestoneStore
{
    public static IFloorPlanMilestoneStore NewMemoryFloorPlanMilestoneStore()
    {
        return new MemoryFloorPlanMilestoneStore(new Dictionary<string, FloorPlanMilestone>(), new List<string>());
    }

    public static IFloorPlanMilestoneStore NewCachedFloorPlanMilestoneStore(MemoryFloorPlanMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanMilestoneStore(inner, FloorPlanMilestoneCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FloorPlanMilestone> FindFloorPlanMilestones(IFloorPlanMilestoneStore store, FloorPlanMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
