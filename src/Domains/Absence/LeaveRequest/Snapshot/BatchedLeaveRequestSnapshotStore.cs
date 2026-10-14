namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched leave request snapshots by code.</summary>
public interface IBatchedLeaveRequestSnapshotStore
{
    BatchedLeaveRequestSnapshot? Find(string code);
    void Save(BatchedLeaveRequestSnapshot item);
    bool Remove(string code);
    List<BatchedLeaveRequestSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedLeaveRequestSnapshotStore(Dictionary<string, BatchedLeaveRequestSnapshot> items, List<string> order) : IBatchedLeaveRequestSnapshotStore
{
    public Dictionary<string, BatchedLeaveRequestSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedLeaveRequestSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedLeaveRequestSnapshot item)
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

    public List<BatchedLeaveRequestSnapshot> List()
    {
        var outValue = new List<BatchedLeaveRequestSnapshot>();
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
public sealed class CachedBatchedLeaveRequestSnapshotStore(MemoryBatchedLeaveRequestSnapshotStore inner, BatchedLeaveRequestSnapshotCache cache, int hits, int misses) : IBatchedLeaveRequestSnapshotStore
{
    public MemoryBatchedLeaveRequestSnapshotStore Inner { get; set; } = inner;
    public BatchedLeaveRequestSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedLeaveRequestSnapshot? Find(string code)
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

    public void Save(BatchedLeaveRequestSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedLeaveRequestSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched leave request snapshots. Empty fields match all.</summary>
public sealed record BatchedLeaveRequestSnapshotQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedLeaveRequestSnapshot item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedLeaveRequestSnapshot> Apply(List<BatchedLeaveRequestSnapshot> items)
    {
        var matched = Collections.Filter(items, (BatchedLeaveRequestSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched leave request snapshot store.</summary>
public static class BatchedLeaveRequestSnapshotStore
{
    public static IBatchedLeaveRequestSnapshotStore NewMemoryBatchedLeaveRequestSnapshotStore()
    {
        return new MemoryBatchedLeaveRequestSnapshotStore(
            new Dictionary<string, BatchedLeaveRequestSnapshot>(),
            new List<string>());
    }

    public static IBatchedLeaveRequestSnapshotStore NewCachedBatchedLeaveRequestSnapshotStore(MemoryBatchedLeaveRequestSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedLeaveRequestSnapshotStore(
            inner,
            BatchedLeaveRequestSnapshotCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedLeaveRequestSnapshot> FindBatchedLeaveRequestSnapshots(IBatchedLeaveRequestSnapshotStore store, BatchedLeaveRequestSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
