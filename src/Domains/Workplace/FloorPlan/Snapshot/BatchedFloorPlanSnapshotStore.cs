namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched floor plan snapshots by code.</summary>
public interface IBatchedFloorPlanSnapshotStore
{
    BatchedFloorPlanSnapshot? Find(string code);
    void Save(BatchedFloorPlanSnapshot item);
    bool Remove(string code);
    List<BatchedFloorPlanSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedFloorPlanSnapshotStore(Dictionary<string, BatchedFloorPlanSnapshot> items, List<string> order) : IBatchedFloorPlanSnapshotStore
{
    public Dictionary<string, BatchedFloorPlanSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedFloorPlanSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedFloorPlanSnapshot item)
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

    public List<BatchedFloorPlanSnapshot> List()
    {
        var outValue = new List<BatchedFloorPlanSnapshot>();
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
public sealed class CachedBatchedFloorPlanSnapshotStore(MemoryBatchedFloorPlanSnapshotStore inner, BatchedFloorPlanSnapshotCache cache, int hits, int misses) : IBatchedFloorPlanSnapshotStore
{
    public MemoryBatchedFloorPlanSnapshotStore Inner { get; set; } = inner;
    public BatchedFloorPlanSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedFloorPlanSnapshot? Find(string code)
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

    public void Save(BatchedFloorPlanSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedFloorPlanSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched floor plan snapshots. Empty fields match all.</summary>
public sealed record BatchedFloorPlanSnapshotQuery(string Country, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedFloorPlanSnapshot item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedFloorPlanSnapshot> Apply(List<BatchedFloorPlanSnapshot> items)
    {
        var matched = Collections.Filter(items, (BatchedFloorPlanSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched floor plan snapshot store.</summary>
public static class BatchedFloorPlanSnapshotStore
{
    public static IBatchedFloorPlanSnapshotStore NewMemoryBatchedFloorPlanSnapshotStore()
    {
        return new MemoryBatchedFloorPlanSnapshotStore(
            new Dictionary<string, BatchedFloorPlanSnapshot>(),
            new List<string>());
    }

    public static IBatchedFloorPlanSnapshotStore NewCachedBatchedFloorPlanSnapshotStore(MemoryBatchedFloorPlanSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedFloorPlanSnapshotStore(
            inner,
            BatchedFloorPlanSnapshotCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedFloorPlanSnapshot> FindBatchedFloorPlanSnapshots(IBatchedFloorPlanSnapshotStore store, BatchedFloorPlanSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
