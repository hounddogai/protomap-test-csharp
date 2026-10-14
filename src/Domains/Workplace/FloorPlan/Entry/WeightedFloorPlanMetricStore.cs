namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted floor plan metrics by code.</summary>
public interface IWeightedFloorPlanMetricStore
{
    WeightedFloorPlanMetric? Find(string code);
    void Save(WeightedFloorPlanMetric item);
    bool Remove(string code);
    List<WeightedFloorPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedFloorPlanMetricStore(Dictionary<string, WeightedFloorPlanMetric> items, List<string> order) : IWeightedFloorPlanMetricStore
{
    public Dictionary<string, WeightedFloorPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedFloorPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedFloorPlanMetric item)
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

    public List<WeightedFloorPlanMetric> List()
    {
        var outValue = new List<WeightedFloorPlanMetric>();
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
public sealed class CachedWeightedFloorPlanMetricStore(MemoryWeightedFloorPlanMetricStore inner, WeightedFloorPlanMetricCache cache, int hits, int misses) : IWeightedFloorPlanMetricStore
{
    public MemoryWeightedFloorPlanMetricStore Inner { get; set; } = inner;
    public WeightedFloorPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedFloorPlanMetric? Find(string code)
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

    public void Save(WeightedFloorPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedFloorPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted floor plan metrics. Empty fields match all.</summary>
public sealed record WeightedFloorPlanMetricQuery(string Channel, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedFloorPlanMetric item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedFloorPlanMetric> Apply(List<WeightedFloorPlanMetric> items)
    {
        var matched = Collections.Filter(items, (WeightedFloorPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted floor plan metric store.</summary>
public static class WeightedFloorPlanMetricStore
{
    public static IWeightedFloorPlanMetricStore NewMemoryWeightedFloorPlanMetricStore()
    {
        return new MemoryWeightedFloorPlanMetricStore(
            new Dictionary<string, WeightedFloorPlanMetric>(),
            new List<string>());
    }

    public static IWeightedFloorPlanMetricStore NewCachedWeightedFloorPlanMetricStore(MemoryWeightedFloorPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedFloorPlanMetricStore(
            inner,
            WeightedFloorPlanMetricCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedFloorPlanMetric> FindWeightedFloorPlanMetrics(IWeightedFloorPlanMetricStore store, WeightedFloorPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
