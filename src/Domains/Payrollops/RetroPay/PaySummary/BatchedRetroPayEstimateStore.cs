namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched retro pay estimates by code.</summary>
public interface IBatchedRetroPayEstimateStore
{
    BatchedRetroPayEstimate? Find(string code);
    void Save(BatchedRetroPayEstimate item);
    bool Remove(string code);
    List<BatchedRetroPayEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedRetroPayEstimateStore(Dictionary<string, BatchedRetroPayEstimate> items, List<string> order) : IBatchedRetroPayEstimateStore
{
    public Dictionary<string, BatchedRetroPayEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedRetroPayEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedRetroPayEstimate item)
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

    public List<BatchedRetroPayEstimate> List()
    {
        var outValue = new List<BatchedRetroPayEstimate>();
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
public sealed class CachedBatchedRetroPayEstimateStore(MemoryBatchedRetroPayEstimateStore inner, BatchedRetroPayEstimateCache cache, int hits, int misses) : IBatchedRetroPayEstimateStore
{
    public MemoryBatchedRetroPayEstimateStore Inner { get; set; } = inner;
    public BatchedRetroPayEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedRetroPayEstimate? Find(string code)
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

    public void Save(BatchedRetroPayEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedRetroPayEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched retro pay estimates. Empty fields match all.</summary>
public sealed record BatchedRetroPayEstimateQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedRetroPayEstimate item)
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

    public List<BatchedRetroPayEstimate> Apply(List<BatchedRetroPayEstimate> items)
    {
        var matched = Collections.Filter(items, (BatchedRetroPayEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched retro pay estimate store.</summary>
public static class BatchedRetroPayEstimateStore
{
    public static IBatchedRetroPayEstimateStore NewMemoryBatchedRetroPayEstimateStore()
    {
        return new MemoryBatchedRetroPayEstimateStore(
            new Dictionary<string, BatchedRetroPayEstimate>(),
            new List<string>());
    }

    public static IBatchedRetroPayEstimateStore NewCachedBatchedRetroPayEstimateStore(MemoryBatchedRetroPayEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedRetroPayEstimateStore(
            inner,
            BatchedRetroPayEstimateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedRetroPayEstimate> FindBatchedRetroPayEstimates(IBatchedRetroPayEstimateStore store, BatchedRetroPayEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
