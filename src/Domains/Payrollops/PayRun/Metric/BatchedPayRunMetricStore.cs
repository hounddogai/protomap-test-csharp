namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched pay run metrics by code.</summary>
public interface IBatchedPayRunMetricStore
{
    BatchedPayRunMetric? Find(string code);
    void Save(BatchedPayRunMetric item);
    bool Remove(string code);
    List<BatchedPayRunMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedPayRunMetricStore(Dictionary<string, BatchedPayRunMetric> items, List<string> order) : IBatchedPayRunMetricStore
{
    public Dictionary<string, BatchedPayRunMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedPayRunMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedPayRunMetric item)
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

    public List<BatchedPayRunMetric> List()
    {
        var outValue = new List<BatchedPayRunMetric>();
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
public sealed class CachedBatchedPayRunMetricStore(MemoryBatchedPayRunMetricStore inner, BatchedPayRunMetricCache cache, int hits, int misses) : IBatchedPayRunMetricStore
{
    public MemoryBatchedPayRunMetricStore Inner { get; set; } = inner;
    public BatchedPayRunMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedPayRunMetric? Find(string code)
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

    public void Save(BatchedPayRunMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedPayRunMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched pay run metrics. Empty fields match all.</summary>
public sealed record BatchedPayRunMetricQuery(string Locale, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedPayRunMetric item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<BatchedPayRunMetric> Apply(List<BatchedPayRunMetric> items)
    {
        var matched = Collections.Filter(items, (BatchedPayRunMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched pay run metric store.</summary>
public static class BatchedPayRunMetricStore
{
    public static IBatchedPayRunMetricStore NewMemoryBatchedPayRunMetricStore()
    {
        return new MemoryBatchedPayRunMetricStore(new Dictionary<string, BatchedPayRunMetric>(), new List<string>());
    }

    public static IBatchedPayRunMetricStore NewCachedBatchedPayRunMetricStore(MemoryBatchedPayRunMetricStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedPayRunMetricStore(inner, BatchedPayRunMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BatchedPayRunMetric> FindBatchedPayRunMetrics(IBatchedPayRunMetricStore store, BatchedPayRunMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
