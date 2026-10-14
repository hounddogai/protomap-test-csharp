namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh metrics by code.</summary>
public interface IRefreshMetricStore
{
    RefreshMetric? Find(string code);
    void Save(RefreshMetric item);
    bool Remove(string code);
    List<RefreshMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshMetricStore(Dictionary<string, RefreshMetric> items, List<string> order) : IRefreshMetricStore
{
    public Dictionary<string, RefreshMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshMetric item)
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

    public List<RefreshMetric> List()
    {
        var outValue = new List<RefreshMetric>();
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
public sealed class CachedRefreshMetricStore(MemoryRefreshMetricStore inner, RefreshMetricCache cache, int hits, int misses) : IRefreshMetricStore
{
    public MemoryRefreshMetricStore Inner { get; set; } = inner;
    public RefreshMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshMetric? Find(string code)
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

    public void Save(RefreshMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh metrics. Empty fields match all.</summary>
public sealed record RefreshMetricQuery(string Locale, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshMetric item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshMetric> Apply(List<RefreshMetric> items)
    {
        var matched = Collections.Filter(items, (RefreshMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh metric store.</summary>
public static class RefreshMetricStore
{
    public static IRefreshMetricStore NewMemoryRefreshMetricStore()
    {
        return new MemoryRefreshMetricStore(new Dictionary<string, RefreshMetric>(), new List<string>());
    }

    public static IRefreshMetricStore NewCachedRefreshMetricStore(MemoryRefreshMetricStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshMetricStore(inner, RefreshMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RefreshMetric> FindRefreshMetrics(IRefreshMetricStore store, RefreshMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
