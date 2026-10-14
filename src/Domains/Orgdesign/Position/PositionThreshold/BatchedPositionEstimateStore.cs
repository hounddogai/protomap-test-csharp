namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched position estimates by code.</summary>
public interface IBatchedPositionEstimateStore
{
    BatchedPositionEstimate? Find(string code);
    void Save(BatchedPositionEstimate item);
    bool Remove(string code);
    List<BatchedPositionEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedPositionEstimateStore(Dictionary<string, BatchedPositionEstimate> items, List<string> order) : IBatchedPositionEstimateStore
{
    public Dictionary<string, BatchedPositionEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedPositionEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedPositionEstimate item)
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

    public List<BatchedPositionEstimate> List()
    {
        var outValue = new List<BatchedPositionEstimate>();
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
public sealed class CachedBatchedPositionEstimateStore(MemoryBatchedPositionEstimateStore inner, BatchedPositionEstimateCache cache, int hits, int misses) : IBatchedPositionEstimateStore
{
    public MemoryBatchedPositionEstimateStore Inner { get; set; } = inner;
    public BatchedPositionEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedPositionEstimate? Find(string code)
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

    public void Save(BatchedPositionEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedPositionEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched position estimates. Empty fields match all.</summary>
public sealed record BatchedPositionEstimateQuery(string Name, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedPositionEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedPositionEstimate> Apply(List<BatchedPositionEstimate> items)
    {
        var matched = Collections.Filter(items, (BatchedPositionEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched position estimate store.</summary>
public static class BatchedPositionEstimateStore
{
    public static IBatchedPositionEstimateStore NewMemoryBatchedPositionEstimateStore()
    {
        return new MemoryBatchedPositionEstimateStore(
            new Dictionary<string, BatchedPositionEstimate>(),
            new List<string>());
    }

    public static IBatchedPositionEstimateStore NewCachedBatchedPositionEstimateStore(MemoryBatchedPositionEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedPositionEstimateStore(
            inner,
            BatchedPositionEstimateCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedPositionEstimate> FindBatchedPositionEstimates(IBatchedPositionEstimateStore store, BatchedPositionEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
