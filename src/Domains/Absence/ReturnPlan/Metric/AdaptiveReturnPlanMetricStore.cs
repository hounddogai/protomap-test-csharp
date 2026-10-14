namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive return plan metrics by code.</summary>
public interface IAdaptiveReturnPlanMetricStore
{
    AdaptiveReturnPlanMetric? Find(string code);
    void Save(AdaptiveReturnPlanMetric item);
    bool Remove(string code);
    List<AdaptiveReturnPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveReturnPlanMetricStore(Dictionary<string, AdaptiveReturnPlanMetric> items, List<string> order) : IAdaptiveReturnPlanMetricStore
{
    public Dictionary<string, AdaptiveReturnPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveReturnPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveReturnPlanMetric item)
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

    public List<AdaptiveReturnPlanMetric> List()
    {
        var outValue = new List<AdaptiveReturnPlanMetric>();
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
public sealed class CachedAdaptiveReturnPlanMetricStore(MemoryAdaptiveReturnPlanMetricStore inner, AdaptiveReturnPlanMetricCache cache, int hits, int misses) : IAdaptiveReturnPlanMetricStore
{
    public MemoryAdaptiveReturnPlanMetricStore Inner { get; set; } = inner;
    public AdaptiveReturnPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveReturnPlanMetric? Find(string code)
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

    public void Save(AdaptiveReturnPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveReturnPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive return plan metrics. Empty fields match all.</summary>
public sealed record AdaptiveReturnPlanMetricQuery(string Team, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveReturnPlanMetric item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveReturnPlanMetric> Apply(List<AdaptiveReturnPlanMetric> items)
    {
        var matched = Collections.Filter(items, (AdaptiveReturnPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive return plan metric store.</summary>
public static class AdaptiveReturnPlanMetricStore
{
    public static IAdaptiveReturnPlanMetricStore NewMemoryAdaptiveReturnPlanMetricStore()
    {
        return new MemoryAdaptiveReturnPlanMetricStore(
            new Dictionary<string, AdaptiveReturnPlanMetric>(),
            new List<string>());
    }

    public static IAdaptiveReturnPlanMetricStore NewCachedAdaptiveReturnPlanMetricStore(MemoryAdaptiveReturnPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveReturnPlanMetricStore(
            inner,
            AdaptiveReturnPlanMetricCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveReturnPlanMetric> FindAdaptiveReturnPlanMetrics(IAdaptiveReturnPlanMetricStore store, AdaptiveReturnPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
