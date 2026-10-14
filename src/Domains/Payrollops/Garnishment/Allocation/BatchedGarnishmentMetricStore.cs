namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched garnishment metrics by code.</summary>
public interface IBatchedGarnishmentMetricStore
{
    BatchedGarnishmentMetric? Find(string code);
    void Save(BatchedGarnishmentMetric item);
    bool Remove(string code);
    List<BatchedGarnishmentMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedGarnishmentMetricStore(Dictionary<string, BatchedGarnishmentMetric> items, List<string> order) : IBatchedGarnishmentMetricStore
{
    public Dictionary<string, BatchedGarnishmentMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedGarnishmentMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedGarnishmentMetric item)
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

    public List<BatchedGarnishmentMetric> List()
    {
        var outValue = new List<BatchedGarnishmentMetric>();
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
public sealed class CachedBatchedGarnishmentMetricStore(MemoryBatchedGarnishmentMetricStore inner, BatchedGarnishmentMetricCache cache, int hits, int misses) : IBatchedGarnishmentMetricStore
{
    public MemoryBatchedGarnishmentMetricStore Inner { get; set; } = inner;
    public BatchedGarnishmentMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedGarnishmentMetric? Find(string code)
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

    public void Save(BatchedGarnishmentMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedGarnishmentMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched garnishment metrics. Empty fields match all.</summary>
public sealed record BatchedGarnishmentMetricQuery(string Name, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedGarnishmentMetric item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedGarnishmentMetric> Apply(List<BatchedGarnishmentMetric> items)
    {
        var matched = Collections.Filter(items, (BatchedGarnishmentMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched garnishment metric store.</summary>
public static class BatchedGarnishmentMetricStore
{
    public static IBatchedGarnishmentMetricStore NewMemoryBatchedGarnishmentMetricStore()
    {
        return new MemoryBatchedGarnishmentMetricStore(
            new Dictionary<string, BatchedGarnishmentMetric>(),
            new List<string>());
    }

    public static IBatchedGarnishmentMetricStore NewCachedBatchedGarnishmentMetricStore(MemoryBatchedGarnishmentMetricStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedGarnishmentMetricStore(
            inner,
            BatchedGarnishmentMetricCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<BatchedGarnishmentMetric> FindBatchedGarnishmentMetrics(IBatchedGarnishmentMetricStore store, BatchedGarnishmentMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
