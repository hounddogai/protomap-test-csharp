namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan thresholds by code.</summary>
public interface IFloorPlanThresholdStore
{
    FloorPlanThreshold? Find(string code);
    void Save(FloorPlanThreshold item);
    bool Remove(string code);
    List<FloorPlanThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanThresholdStore(Dictionary<string, FloorPlanThreshold> items, List<string> order) : IFloorPlanThresholdStore
{
    public Dictionary<string, FloorPlanThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanThreshold item)
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

    public List<FloorPlanThreshold> List()
    {
        var outValue = new List<FloorPlanThreshold>();
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
public sealed class CachedFloorPlanThresholdStore(MemoryFloorPlanThresholdStore inner, FloorPlanThresholdCache cache, int hits, int misses) : IFloorPlanThresholdStore
{
    public MemoryFloorPlanThresholdStore Inner { get; set; } = inner;
    public FloorPlanThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanThreshold? Find(string code)
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

    public void Save(FloorPlanThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan thresholds. Empty fields match all.</summary>
public sealed record FloorPlanThresholdQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanThreshold item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanThreshold> Apply(List<FloorPlanThreshold> items)
    {
        var matched = Collections.Filter(items, (FloorPlanThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan threshold store.</summary>
public static class FloorPlanThresholdStore
{
    public static IFloorPlanThresholdStore NewMemoryFloorPlanThresholdStore()
    {
        return new MemoryFloorPlanThresholdStore(new Dictionary<string, FloorPlanThreshold>(), new List<string>());
    }

    public static IFloorPlanThresholdStore NewCachedFloorPlanThresholdStore(MemoryFloorPlanThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanThresholdStore(inner, FloorPlanThresholdCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FloorPlanThreshold> FindFloorPlanThresholds(IFloorPlanThresholdStore store, FloorPlanThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
