namespace Acme.Workforce.Absence.Accrual.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched accrual checkpoints by code.</summary>
public interface IBatchedAccrualCheckpointStore
{
    BatchedAccrualCheckpoint? Find(string code);
    void Save(BatchedAccrualCheckpoint item);
    bool Remove(string code);
    List<BatchedAccrualCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAccrualCheckpointStore(Dictionary<string, BatchedAccrualCheckpoint> items, List<string> order) : IBatchedAccrualCheckpointStore
{
    public Dictionary<string, BatchedAccrualCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAccrualCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAccrualCheckpoint item)
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

    public List<BatchedAccrualCheckpoint> List()
    {
        var outValue = new List<BatchedAccrualCheckpoint>();
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
public sealed class CachedBatchedAccrualCheckpointStore(MemoryBatchedAccrualCheckpointStore inner, BatchedAccrualCheckpointCache cache, int hits, int misses) : IBatchedAccrualCheckpointStore
{
    public MemoryBatchedAccrualCheckpointStore Inner { get; set; } = inner;
    public BatchedAccrualCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAccrualCheckpoint? Find(string code)
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

    public void Save(BatchedAccrualCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAccrualCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched accrual checkpoints. Empty fields match all.</summary>
public sealed record BatchedAccrualCheckpointQuery(string Source, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAccrualCheckpoint item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedAccrualCheckpoint> Apply(List<BatchedAccrualCheckpoint> items)
    {
        var matched = Collections.Filter(items, (BatchedAccrualCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched accrual checkpoint store.</summary>
public static class BatchedAccrualCheckpointStore
{
    public static IBatchedAccrualCheckpointStore NewMemoryBatchedAccrualCheckpointStore()
    {
        return new MemoryBatchedAccrualCheckpointStore(
            new Dictionary<string, BatchedAccrualCheckpoint>(),
            new List<string>());
    }

    public static IBatchedAccrualCheckpointStore NewCachedBatchedAccrualCheckpointStore(MemoryBatchedAccrualCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAccrualCheckpointStore(
            inner,
            BatchedAccrualCheckpointCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedAccrualCheckpoint> FindBatchedAccrualCheckpoints(IBatchedAccrualCheckpointStore store, BatchedAccrualCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
