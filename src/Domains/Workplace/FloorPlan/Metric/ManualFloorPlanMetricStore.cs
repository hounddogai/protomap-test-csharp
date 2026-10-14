namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual floor plan metrics by code.</summary>
public interface IManualFloorPlanMetricStore
{
    ManualFloorPlanMetric? Find(string code);
    void Save(ManualFloorPlanMetric item);
    bool Remove(string code);
    List<ManualFloorPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualFloorPlanMetricStore(Dictionary<string, ManualFloorPlanMetric> items, List<string> order) : IManualFloorPlanMetricStore
{
    public Dictionary<string, ManualFloorPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualFloorPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualFloorPlanMetric item)
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

    public List<ManualFloorPlanMetric> List()
    {
        var outValue = new List<ManualFloorPlanMetric>();
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
public sealed class CachedManualFloorPlanMetricStore(MemoryManualFloorPlanMetricStore inner, ManualFloorPlanMetricCache cache, int hits, int misses) : IManualFloorPlanMetricStore
{
    public MemoryManualFloorPlanMetricStore Inner { get; set; } = inner;
    public ManualFloorPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualFloorPlanMetric? Find(string code)
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

    public void Save(ManualFloorPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualFloorPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual floor plan metrics. Empty fields match all.</summary>
public sealed record ManualFloorPlanMetricQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(ManualFloorPlanMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualFloorPlanMetric> Apply(List<ManualFloorPlanMetric> items)
    {
        var matched = Collections.Filter(items, (ManualFloorPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual floor plan metric store.</summary>
public static class ManualFloorPlanMetricStore
{
    public static IManualFloorPlanMetricStore NewMemoryManualFloorPlanMetricStore()
    {
        return new MemoryManualFloorPlanMetricStore(
            new Dictionary<string, ManualFloorPlanMetric>(),
            new List<string>());
    }

    public static IManualFloorPlanMetricStore NewCachedManualFloorPlanMetricStore(MemoryManualFloorPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedManualFloorPlanMetricStore(inner, ManualFloorPlanMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ManualFloorPlanMetric> FindManualFloorPlanMetrics(IManualFloorPlanMetricStore store, ManualFloorPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
