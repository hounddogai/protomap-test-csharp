namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual benchmark metrics by code.</summary>
public interface IManualBenchmarkMetricStore
{
    ManualBenchmarkMetric? Find(string code);
    void Save(ManualBenchmarkMetric item);
    bool Remove(string code);
    List<ManualBenchmarkMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBenchmarkMetricStore(Dictionary<string, ManualBenchmarkMetric> items, List<string> order) : IManualBenchmarkMetricStore
{
    public Dictionary<string, ManualBenchmarkMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBenchmarkMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBenchmarkMetric item)
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

    public List<ManualBenchmarkMetric> List()
    {
        var outValue = new List<ManualBenchmarkMetric>();
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
public sealed class CachedManualBenchmarkMetricStore(MemoryManualBenchmarkMetricStore inner, ManualBenchmarkMetricCache cache, int hits, int misses) : IManualBenchmarkMetricStore
{
    public MemoryManualBenchmarkMetricStore Inner { get; set; } = inner;
    public ManualBenchmarkMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBenchmarkMetric? Find(string code)
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

    public void Save(ManualBenchmarkMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBenchmarkMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual benchmark metrics. Empty fields match all.</summary>
public sealed record ManualBenchmarkMetricQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBenchmarkMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBenchmarkMetric> Apply(List<ManualBenchmarkMetric> items)
    {
        var matched = Collections.Filter(items, (ManualBenchmarkMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual benchmark metric store.</summary>
public static class ManualBenchmarkMetricStore
{
    public static IManualBenchmarkMetricStore NewMemoryManualBenchmarkMetricStore()
    {
        return new MemoryManualBenchmarkMetricStore(
            new Dictionary<string, ManualBenchmarkMetric>(),
            new List<string>());
    }

    public static IManualBenchmarkMetricStore NewCachedManualBenchmarkMetricStore(MemoryManualBenchmarkMetricStore inner, int ttl, IClock clock)
    {
        return new CachedManualBenchmarkMetricStore(inner, ManualBenchmarkMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ManualBenchmarkMetric> FindManualBenchmarkMetrics(IManualBenchmarkMetricStore store, ManualBenchmarkMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
