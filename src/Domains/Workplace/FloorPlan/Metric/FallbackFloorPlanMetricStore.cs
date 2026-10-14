namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback floor plan metrics by code.</summary>
public interface IFallbackFloorPlanMetricStore
{
    FallbackFloorPlanMetric? Find(string code);
    void Save(FallbackFloorPlanMetric item);
    bool Remove(string code);
    List<FallbackFloorPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackFloorPlanMetricStore(Dictionary<string, FallbackFloorPlanMetric> items, List<string> order) : IFallbackFloorPlanMetricStore
{
    public Dictionary<string, FallbackFloorPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackFloorPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackFloorPlanMetric item)
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

    public List<FallbackFloorPlanMetric> List()
    {
        var outValue = new List<FallbackFloorPlanMetric>();
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
public sealed class CachedFallbackFloorPlanMetricStore(MemoryFallbackFloorPlanMetricStore inner, FallbackFloorPlanMetricCache cache, int hits, int misses) : IFallbackFloorPlanMetricStore
{
    public MemoryFallbackFloorPlanMetricStore Inner { get; set; } = inner;
    public FallbackFloorPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackFloorPlanMetric? Find(string code)
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

    public void Save(FallbackFloorPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackFloorPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback floor plan metrics. Empty fields match all.</summary>
public sealed record FallbackFloorPlanMetricQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackFloorPlanMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackFloorPlanMetric> Apply(List<FallbackFloorPlanMetric> items)
    {
        var matched = Collections.Filter(items, (FallbackFloorPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback floor plan metric store.</summary>
public static class FallbackFloorPlanMetricStore
{
    public static IFallbackFloorPlanMetricStore NewMemoryFallbackFloorPlanMetricStore()
    {
        return new MemoryFallbackFloorPlanMetricStore(
            new Dictionary<string, FallbackFloorPlanMetric>(),
            new List<string>());
    }

    public static IFallbackFloorPlanMetricStore NewCachedFallbackFloorPlanMetricStore(MemoryFallbackFloorPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackFloorPlanMetricStore(
            inner,
            FallbackFloorPlanMetricCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackFloorPlanMetric> FindFallbackFloorPlanMetrics(IFallbackFloorPlanMetricStore store, FallbackFloorPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
