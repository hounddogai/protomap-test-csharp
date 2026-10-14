namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan metrics by code.</summary>
public interface IFloorPlanMetricStore
{
    FloorPlanMetric? Find(string code);
    void Save(FloorPlanMetric item);
    bool Remove(string code);
    List<FloorPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanMetricStore(Dictionary<string, FloorPlanMetric> items, List<string> order) : IFloorPlanMetricStore
{
    public Dictionary<string, FloorPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanMetric item)
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

    public List<FloorPlanMetric> List()
    {
        var outValue = new List<FloorPlanMetric>();
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
public sealed class CachedFloorPlanMetricStore(MemoryFloorPlanMetricStore inner, FloorPlanMetricCache cache, int hits, int misses) : IFloorPlanMetricStore
{
    public MemoryFloorPlanMetricStore Inner { get; set; } = inner;
    public FloorPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanMetric? Find(string code)
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

    public void Save(FloorPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan metrics. Empty fields match all.</summary>
public sealed record FloorPlanMetricQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanMetric item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanMetric> Apply(List<FloorPlanMetric> items)
    {
        var matched = Collections.Filter(items, (FloorPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan metric store.</summary>
public static class FloorPlanMetricStore
{
    public static IFloorPlanMetricStore NewMemoryFloorPlanMetricStore()
    {
        return new MemoryFloorPlanMetricStore(new Dictionary<string, FloorPlanMetric>(), new List<string>());
    }

    public static IFloorPlanMetricStore NewCachedFloorPlanMetricStore(MemoryFloorPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanMetricStore(inner, FloorPlanMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FloorPlanMetric> FindFloorPlanMetrics(IFloorPlanMetricStore store, FloorPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
