namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive visitor metrics by code.</summary>
public interface IAdaptiveVisitorMetricStore
{
    AdaptiveVisitorMetric? Find(string code);
    void Save(AdaptiveVisitorMetric item);
    bool Remove(string code);
    List<AdaptiveVisitorMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveVisitorMetricStore(Dictionary<string, AdaptiveVisitorMetric> items, List<string> order) : IAdaptiveVisitorMetricStore
{
    public Dictionary<string, AdaptiveVisitorMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveVisitorMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveVisitorMetric item)
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

    public List<AdaptiveVisitorMetric> List()
    {
        var outValue = new List<AdaptiveVisitorMetric>();
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
public sealed class CachedAdaptiveVisitorMetricStore(MemoryAdaptiveVisitorMetricStore inner, AdaptiveVisitorMetricCache cache, int hits, int misses) : IAdaptiveVisitorMetricStore
{
    public MemoryAdaptiveVisitorMetricStore Inner { get; set; } = inner;
    public AdaptiveVisitorMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveVisitorMetric? Find(string code)
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

    public void Save(AdaptiveVisitorMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveVisitorMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive visitor metrics. Empty fields match all.</summary>
public sealed record AdaptiveVisitorMetricQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveVisitorMetric item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveVisitorMetric> Apply(List<AdaptiveVisitorMetric> items)
    {
        var matched = Collections.Filter(items, (AdaptiveVisitorMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive visitor metric store.</summary>
public static class AdaptiveVisitorMetricStore
{
    public static IAdaptiveVisitorMetricStore NewMemoryAdaptiveVisitorMetricStore()
    {
        return new MemoryAdaptiveVisitorMetricStore(
            new Dictionary<string, AdaptiveVisitorMetric>(),
            new List<string>());
    }

    public static IAdaptiveVisitorMetricStore NewCachedAdaptiveVisitorMetricStore(MemoryAdaptiveVisitorMetricStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveVisitorMetricStore(inner, AdaptiveVisitorMetricCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AdaptiveVisitorMetric> FindAdaptiveVisitorMetrics(IAdaptiveVisitorMetricStore store, AdaptiveVisitorMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
