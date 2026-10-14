namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched payslip metrics by code.</summary>
public interface IBatchedPayslipMetricStore
{
    BatchedPayslipMetric? Find(string code);
    void Save(BatchedPayslipMetric item);
    bool Remove(string code);
    List<BatchedPayslipMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedPayslipMetricStore(Dictionary<string, BatchedPayslipMetric> items, List<string> order) : IBatchedPayslipMetricStore
{
    public Dictionary<string, BatchedPayslipMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedPayslipMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedPayslipMetric item)
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

    public List<BatchedPayslipMetric> List()
    {
        var outValue = new List<BatchedPayslipMetric>();
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
public sealed class CachedBatchedPayslipMetricStore(MemoryBatchedPayslipMetricStore inner, BatchedPayslipMetricCache cache, int hits, int misses) : IBatchedPayslipMetricStore
{
    public MemoryBatchedPayslipMetricStore Inner { get; set; } = inner;
    public BatchedPayslipMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedPayslipMetric? Find(string code)
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

    public void Save(BatchedPayslipMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedPayslipMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched payslip metrics. Empty fields match all.</summary>
public sealed record BatchedPayslipMetricQuery(string Unit, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedPayslipMetric item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<BatchedPayslipMetric> Apply(List<BatchedPayslipMetric> items)
    {
        var matched = Collections.Filter(items, (BatchedPayslipMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched payslip metric store.</summary>
public static class BatchedPayslipMetricStore
{
    public static IBatchedPayslipMetricStore NewMemoryBatchedPayslipMetricStore()
    {
        return new MemoryBatchedPayslipMetricStore(
            new Dictionary<string, BatchedPayslipMetric>(),
            new List<string>());
    }

    public static IBatchedPayslipMetricStore NewCachedBatchedPayslipMetricStore(MemoryBatchedPayslipMetricStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedPayslipMetricStore(inner, BatchedPayslipMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BatchedPayslipMetric> FindBatchedPayslipMetrics(IBatchedPayslipMetricStore store, BatchedPayslipMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
