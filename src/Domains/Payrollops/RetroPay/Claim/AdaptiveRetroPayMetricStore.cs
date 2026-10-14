namespace Acme.Workforce.Payrollops.RetroPay.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive retro pay metrics by code.</summary>
public interface IAdaptiveRetroPayMetricStore
{
    AdaptiveRetroPayMetric? Find(string code);
    void Save(AdaptiveRetroPayMetric item);
    bool Remove(string code);
    List<AdaptiveRetroPayMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveRetroPayMetricStore(Dictionary<string, AdaptiveRetroPayMetric> items, List<string> order) : IAdaptiveRetroPayMetricStore
{
    public Dictionary<string, AdaptiveRetroPayMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveRetroPayMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveRetroPayMetric item)
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

    public List<AdaptiveRetroPayMetric> List()
    {
        var outValue = new List<AdaptiveRetroPayMetric>();
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
public sealed class CachedAdaptiveRetroPayMetricStore(MemoryAdaptiveRetroPayMetricStore inner, AdaptiveRetroPayMetricCache cache, int hits, int misses) : IAdaptiveRetroPayMetricStore
{
    public MemoryAdaptiveRetroPayMetricStore Inner { get; set; } = inner;
    public AdaptiveRetroPayMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveRetroPayMetric? Find(string code)
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

    public void Save(AdaptiveRetroPayMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveRetroPayMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive retro pay metrics. Empty fields match all.</summary>
public sealed record AdaptiveRetroPayMetricQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveRetroPayMetric item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveRetroPayMetric> Apply(List<AdaptiveRetroPayMetric> items)
    {
        var matched = Collections.Filter(items, (AdaptiveRetroPayMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive retro pay metric store.</summary>
public static class AdaptiveRetroPayMetricStore
{
    public static IAdaptiveRetroPayMetricStore NewMemoryAdaptiveRetroPayMetricStore()
    {
        return new MemoryAdaptiveRetroPayMetricStore(
            new Dictionary<string, AdaptiveRetroPayMetric>(),
            new List<string>());
    }

    public static IAdaptiveRetroPayMetricStore NewCachedAdaptiveRetroPayMetricStore(MemoryAdaptiveRetroPayMetricStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveRetroPayMetricStore(
            inner,
            AdaptiveRetroPayMetricCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveRetroPayMetric> FindAdaptiveRetroPayMetrics(IAdaptiveRetroPayMetricStore store, AdaptiveRetroPayMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
