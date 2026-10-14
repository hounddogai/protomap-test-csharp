namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive accrual metrics by code.</summary>
public interface IAdaptiveAccrualMetricStore
{
    AdaptiveAccrualMetric? Find(string code);
    void Save(AdaptiveAccrualMetric item);
    bool Remove(string code);
    List<AdaptiveAccrualMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveAccrualMetricStore(Dictionary<string, AdaptiveAccrualMetric> items, List<string> order) : IAdaptiveAccrualMetricStore
{
    public Dictionary<string, AdaptiveAccrualMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveAccrualMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveAccrualMetric item)
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

    public List<AdaptiveAccrualMetric> List()
    {
        var outValue = new List<AdaptiveAccrualMetric>();
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
public sealed class CachedAdaptiveAccrualMetricStore(MemoryAdaptiveAccrualMetricStore inner, AdaptiveAccrualMetricCache cache, int hits, int misses) : IAdaptiveAccrualMetricStore
{
    public MemoryAdaptiveAccrualMetricStore Inner { get; set; } = inner;
    public AdaptiveAccrualMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveAccrualMetric? Find(string code)
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

    public void Save(AdaptiveAccrualMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveAccrualMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive accrual metrics. Empty fields match all.</summary>
public sealed record AdaptiveAccrualMetricQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveAccrualMetric item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveAccrualMetric> Apply(List<AdaptiveAccrualMetric> items)
    {
        var matched = Collections.Filter(items, (AdaptiveAccrualMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive accrual metric store.</summary>
public static class AdaptiveAccrualMetricStore
{
    public static IAdaptiveAccrualMetricStore NewMemoryAdaptiveAccrualMetricStore()
    {
        return new MemoryAdaptiveAccrualMetricStore(
            new Dictionary<string, AdaptiveAccrualMetric>(),
            new List<string>());
    }

    public static IAdaptiveAccrualMetricStore NewCachedAdaptiveAccrualMetricStore(MemoryAdaptiveAccrualMetricStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveAccrualMetricStore(
            inner,
            AdaptiveAccrualMetricCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveAccrualMetric> FindAdaptiveAccrualMetrics(IAdaptiveAccrualMetricStore store, AdaptiveAccrualMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
