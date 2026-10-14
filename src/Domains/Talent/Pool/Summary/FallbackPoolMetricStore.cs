namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback pool metrics by code.</summary>
public interface IFallbackPoolMetricStore
{
    FallbackPoolMetric? Find(string code);
    void Save(FallbackPoolMetric item);
    bool Remove(string code);
    List<FallbackPoolMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPoolMetricStore(Dictionary<string, FallbackPoolMetric> items, List<string> order) : IFallbackPoolMetricStore
{
    public Dictionary<string, FallbackPoolMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPoolMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPoolMetric item)
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

    public List<FallbackPoolMetric> List()
    {
        var outValue = new List<FallbackPoolMetric>();
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
public sealed class CachedFallbackPoolMetricStore(MemoryFallbackPoolMetricStore inner, FallbackPoolMetricCache cache, int hits, int misses) : IFallbackPoolMetricStore
{
    public MemoryFallbackPoolMetricStore Inner { get; set; } = inner;
    public FallbackPoolMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPoolMetric? Find(string code)
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

    public void Save(FallbackPoolMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPoolMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback pool metrics. Empty fields match all.</summary>
public sealed record FallbackPoolMetricQuery(string Country, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPoolMetric item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPoolMetric> Apply(List<FallbackPoolMetric> items)
    {
        var matched = Collections.Filter(items, (FallbackPoolMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback pool metric store.</summary>
public static class FallbackPoolMetricStore
{
    public static IFallbackPoolMetricStore NewMemoryFallbackPoolMetricStore()
    {
        return new MemoryFallbackPoolMetricStore(new Dictionary<string, FallbackPoolMetric>(), new List<string>());
    }

    public static IFallbackPoolMetricStore NewCachedFallbackPoolMetricStore(MemoryFallbackPoolMetricStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPoolMetricStore(inner, FallbackPoolMetricCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FallbackPoolMetric> FindFallbackPoolMetrics(IFallbackPoolMetricStore store, FallbackPoolMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
