namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched reorg checkpoints by code.</summary>
public interface IBatchedReorgCheckpointStore
{
    BatchedReorgCheckpoint? Find(string code);
    void Save(BatchedReorgCheckpoint item);
    bool Remove(string code);
    List<BatchedReorgCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedReorgCheckpointStore(Dictionary<string, BatchedReorgCheckpoint> items, List<string> order) : IBatchedReorgCheckpointStore
{
    public Dictionary<string, BatchedReorgCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedReorgCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedReorgCheckpoint item)
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

    public List<BatchedReorgCheckpoint> List()
    {
        var outValue = new List<BatchedReorgCheckpoint>();
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
public sealed class CachedBatchedReorgCheckpointStore(MemoryBatchedReorgCheckpointStore inner, BatchedReorgCheckpointCache cache, int hits, int misses) : IBatchedReorgCheckpointStore
{
    public MemoryBatchedReorgCheckpointStore Inner { get; set; } = inner;
    public BatchedReorgCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedReorgCheckpoint? Find(string code)
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

    public void Save(BatchedReorgCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedReorgCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched reorg checkpoints. Empty fields match all.</summary>
public sealed record BatchedReorgCheckpointQuery(string Segment, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedReorgCheckpoint item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedReorgCheckpoint> Apply(List<BatchedReorgCheckpoint> items)
    {
        var matched = Collections.Filter(items, (BatchedReorgCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched reorg checkpoint store.</summary>
public static class BatchedReorgCheckpointStore
{
    public static IBatchedReorgCheckpointStore NewMemoryBatchedReorgCheckpointStore()
    {
        return new MemoryBatchedReorgCheckpointStore(
            new Dictionary<string, BatchedReorgCheckpoint>(),
            new List<string>());
    }

    public static IBatchedReorgCheckpointStore NewCachedBatchedReorgCheckpointStore(MemoryBatchedReorgCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedReorgCheckpointStore(
            inner,
            BatchedReorgCheckpointCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedReorgCheckpoint> FindBatchedReorgCheckpoints(IBatchedReorgCheckpointStore store, BatchedReorgCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
