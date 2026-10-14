namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched accrual metrics by code.</summary>
public interface IBatchedAccrualMetricStore
{
    BatchedAccrualMetric? Find(string code);
    void Save(BatchedAccrualMetric item);
    bool Remove(string code);
    List<BatchedAccrualMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAccrualMetricStore(Dictionary<string, BatchedAccrualMetric> items, List<string> order) : IBatchedAccrualMetricStore
{
    public Dictionary<string, BatchedAccrualMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAccrualMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAccrualMetric item)
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

    public List<BatchedAccrualMetric> List()
    {
        var outValue = new List<BatchedAccrualMetric>();
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
public sealed class CachedBatchedAccrualMetricStore(MemoryBatchedAccrualMetricStore inner, BatchedAccrualMetricCache cache, int hits, int misses) : IBatchedAccrualMetricStore
{
    public MemoryBatchedAccrualMetricStore Inner { get; set; } = inner;
    public BatchedAccrualMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAccrualMetric? Find(string code)
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

    public void Save(BatchedAccrualMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAccrualMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched accrual metrics. Empty fields match all.</summary>
public sealed record BatchedAccrualMetricQuery(string Department, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAccrualMetric item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<BatchedAccrualMetric> Apply(List<BatchedAccrualMetric> items)
    {
        var matched = Collections.Filter(items, (BatchedAccrualMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched accrual metric store.</summary>
public static class BatchedAccrualMetricStore
{
    public static IBatchedAccrualMetricStore NewMemoryBatchedAccrualMetricStore()
    {
        return new MemoryBatchedAccrualMetricStore(
            new Dictionary<string, BatchedAccrualMetric>(),
            new List<string>());
    }

    public static IBatchedAccrualMetricStore NewCachedBatchedAccrualMetricStore(MemoryBatchedAccrualMetricStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAccrualMetricStore(inner, BatchedAccrualMetricCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BatchedAccrualMetric> FindBatchedAccrualMetrics(IBatchedAccrualMetricStore store, BatchedAccrualMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
