namespace Acme.Workforce.Contractors.Engagement.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched engagement variances by code.</summary>
public interface IBatchedEngagementVarianceStore
{
    BatchedEngagementVariance? Find(string code);
    void Save(BatchedEngagementVariance item);
    bool Remove(string code);
    List<BatchedEngagementVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedEngagementVarianceStore(Dictionary<string, BatchedEngagementVariance> items, List<string> order) : IBatchedEngagementVarianceStore
{
    public Dictionary<string, BatchedEngagementVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedEngagementVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedEngagementVariance item)
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

    public List<BatchedEngagementVariance> List()
    {
        var outValue = new List<BatchedEngagementVariance>();
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
public sealed class CachedBatchedEngagementVarianceStore(MemoryBatchedEngagementVarianceStore inner, BatchedEngagementVarianceCache cache, int hits, int misses) : IBatchedEngagementVarianceStore
{
    public MemoryBatchedEngagementVarianceStore Inner { get; set; } = inner;
    public BatchedEngagementVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedEngagementVariance? Find(string code)
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

    public void Save(BatchedEngagementVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedEngagementVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched engagement variances. Empty fields match all.</summary>
public sealed record BatchedEngagementVarianceQuery(string Country, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedEngagementVariance item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedEngagementVariance> Apply(List<BatchedEngagementVariance> items)
    {
        var matched = Collections.Filter(items, (BatchedEngagementVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched engagement variance store.</summary>
public static class BatchedEngagementVarianceStore
{
    public static IBatchedEngagementVarianceStore NewMemoryBatchedEngagementVarianceStore()
    {
        return new MemoryBatchedEngagementVarianceStore(
            new Dictionary<string, BatchedEngagementVariance>(),
            new List<string>());
    }

    public static IBatchedEngagementVarianceStore NewCachedBatchedEngagementVarianceStore(MemoryBatchedEngagementVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedEngagementVarianceStore(
            inner,
            BatchedEngagementVarianceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<BatchedEngagementVariance> FindBatchedEngagementVariances(IBatchedEngagementVarianceStore store, BatchedEngagementVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
