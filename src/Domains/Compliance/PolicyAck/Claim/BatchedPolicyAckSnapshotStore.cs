namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched policy ack snapshots by code.</summary>
public interface IBatchedPolicyAckSnapshotStore
{
    BatchedPolicyAckSnapshot? Find(string code);
    void Save(BatchedPolicyAckSnapshot item);
    bool Remove(string code);
    List<BatchedPolicyAckSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedPolicyAckSnapshotStore(Dictionary<string, BatchedPolicyAckSnapshot> items, List<string> order) : IBatchedPolicyAckSnapshotStore
{
    public Dictionary<string, BatchedPolicyAckSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedPolicyAckSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedPolicyAckSnapshot item)
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

    public List<BatchedPolicyAckSnapshot> List()
    {
        var outValue = new List<BatchedPolicyAckSnapshot>();
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
public sealed class CachedBatchedPolicyAckSnapshotStore(MemoryBatchedPolicyAckSnapshotStore inner, BatchedPolicyAckSnapshotCache cache, int hits, int misses) : IBatchedPolicyAckSnapshotStore
{
    public MemoryBatchedPolicyAckSnapshotStore Inner { get; set; } = inner;
    public BatchedPolicyAckSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedPolicyAckSnapshot? Find(string code)
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

    public void Save(BatchedPolicyAckSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedPolicyAckSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched policy ack snapshots. Empty fields match all.</summary>
public sealed record BatchedPolicyAckSnapshotQuery(string Title, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedPolicyAckSnapshot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedPolicyAckSnapshot> Apply(List<BatchedPolicyAckSnapshot> items)
    {
        var matched = Collections.Filter(items, (BatchedPolicyAckSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched policy ack snapshot store.</summary>
public static class BatchedPolicyAckSnapshotStore
{
    public static IBatchedPolicyAckSnapshotStore NewMemoryBatchedPolicyAckSnapshotStore()
    {
        return new MemoryBatchedPolicyAckSnapshotStore(
            new Dictionary<string, BatchedPolicyAckSnapshot>(),
            new List<string>());
    }

    public static IBatchedPolicyAckSnapshotStore NewCachedBatchedPolicyAckSnapshotStore(MemoryBatchedPolicyAckSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedPolicyAckSnapshotStore(
            inner,
            BatchedPolicyAckSnapshotCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedPolicyAckSnapshot> FindBatchedPolicyAckSnapshots(IBatchedPolicyAckSnapshotStore store, BatchedPolicyAckSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
