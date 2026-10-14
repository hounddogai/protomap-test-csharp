namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg metrics by code.</summary>
public interface IReorgMetricStore
{
    ReorgMetric? Find(string code);
    void Save(ReorgMetric item);
    bool Remove(string code);
    List<ReorgMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgMetricStore(Dictionary<string, ReorgMetric> items, List<string> order) : IReorgMetricStore
{
    public Dictionary<string, ReorgMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgMetric item)
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

    public List<ReorgMetric> List()
    {
        var outValue = new List<ReorgMetric>();
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
public sealed class CachedReorgMetricStore(MemoryReorgMetricStore inner, ReorgMetricCache cache, int hits, int misses) : IReorgMetricStore
{
    public MemoryReorgMetricStore Inner { get; set; } = inner;
    public ReorgMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgMetric? Find(string code)
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

    public void Save(ReorgMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg metrics. Empty fields match all.</summary>
public sealed record ReorgMetricQuery(string Locale, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgMetric item)
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

    public List<ReorgMetric> Apply(List<ReorgMetric> items)
    {
        var matched = Collections.Filter(items, (ReorgMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg metric store.</summary>
public static class ReorgMetricStore
{
    public static IReorgMetricStore NewMemoryReorgMetricStore()
    {
        return new MemoryReorgMetricStore(new Dictionary<string, ReorgMetric>(), new List<string>());
    }

    public static IReorgMetricStore NewCachedReorgMetricStore(MemoryReorgMetricStore inner, int ttl, IClock clock)
    {
        return new CachedReorgMetricStore(inner, ReorgMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ReorgMetric> FindReorgMetrics(IReorgMetricStore store, ReorgMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
