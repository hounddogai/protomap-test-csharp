namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive floor plan metrics by code.</summary>
public interface IAdaptiveFloorPlanMetricStore
{
    AdaptiveFloorPlanMetric? Find(string code);
    void Save(AdaptiveFloorPlanMetric item);
    bool Remove(string code);
    List<AdaptiveFloorPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveFloorPlanMetricStore(Dictionary<string, AdaptiveFloorPlanMetric> items, List<string> order) : IAdaptiveFloorPlanMetricStore
{
    public Dictionary<string, AdaptiveFloorPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveFloorPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveFloorPlanMetric item)
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

    public List<AdaptiveFloorPlanMetric> List()
    {
        var outValue = new List<AdaptiveFloorPlanMetric>();
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
public sealed class CachedAdaptiveFloorPlanMetricStore(MemoryAdaptiveFloorPlanMetricStore inner, AdaptiveFloorPlanMetricCache cache, int hits, int misses) : IAdaptiveFloorPlanMetricStore
{
    public MemoryAdaptiveFloorPlanMetricStore Inner { get; set; } = inner;
    public AdaptiveFloorPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveFloorPlanMetric? Find(string code)
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

    public void Save(AdaptiveFloorPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveFloorPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive floor plan metrics. Empty fields match all.</summary>
public sealed record AdaptiveFloorPlanMetricQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveFloorPlanMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveFloorPlanMetric> Apply(List<AdaptiveFloorPlanMetric> items)
    {
        var matched = Collections.Filter(items, (AdaptiveFloorPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive floor plan metric store.</summary>
public static class AdaptiveFloorPlanMetricStore
{
    public static IAdaptiveFloorPlanMetricStore NewMemoryAdaptiveFloorPlanMetricStore()
    {
        return new MemoryAdaptiveFloorPlanMetricStore(
            new Dictionary<string, AdaptiveFloorPlanMetric>(),
            new List<string>());
    }

    public static IAdaptiveFloorPlanMetricStore NewCachedAdaptiveFloorPlanMetricStore(MemoryAdaptiveFloorPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveFloorPlanMetricStore(
            inner,
            AdaptiveFloorPlanMetricCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveFloorPlanMetric> FindAdaptiveFloorPlanMetrics(IAdaptiveFloorPlanMetricStore store, AdaptiveFloorPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
