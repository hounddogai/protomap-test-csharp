namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending leave request batches by code.</summary>
public interface IPendingLeaveRequestBatchStore
{
    PendingLeaveRequestBatch? Find(string code);
    void Save(PendingLeaveRequestBatch item);
    bool Remove(string code);
    List<PendingLeaveRequestBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingLeaveRequestBatchStore(Dictionary<string, PendingLeaveRequestBatch> items, List<string> order) : IPendingLeaveRequestBatchStore
{
    public Dictionary<string, PendingLeaveRequestBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingLeaveRequestBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingLeaveRequestBatch item)
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

    public List<PendingLeaveRequestBatch> List()
    {
        var outValue = new List<PendingLeaveRequestBatch>();
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
public sealed class CachedPendingLeaveRequestBatchStore(MemoryPendingLeaveRequestBatchStore inner, PendingLeaveRequestBatchCache cache, int hits, int misses) : IPendingLeaveRequestBatchStore
{
    public MemoryPendingLeaveRequestBatchStore Inner { get; set; } = inner;
    public PendingLeaveRequestBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingLeaveRequestBatch? Find(string code)
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

    public void Save(PendingLeaveRequestBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingLeaveRequestBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending leave request batches. Empty fields match all.</summary>
public sealed record PendingLeaveRequestBatchQuery(string Segment, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PendingLeaveRequestBatch item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingLeaveRequestBatch> Apply(List<PendingLeaveRequestBatch> items)
    {
        var matched = Collections.Filter(items, (PendingLeaveRequestBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending leave request batch store.</summary>
public static class PendingLeaveRequestBatchStore
{
    public static IPendingLeaveRequestBatchStore NewMemoryPendingLeaveRequestBatchStore()
    {
        return new MemoryPendingLeaveRequestBatchStore(
            new Dictionary<string, PendingLeaveRequestBatch>(),
            new List<string>());
    }

    public static IPendingLeaveRequestBatchStore NewCachedPendingLeaveRequestBatchStore(MemoryPendingLeaveRequestBatchStore inner, int ttl, IClock clock)
    {
        return new CachedPendingLeaveRequestBatchStore(
            inner,
            PendingLeaveRequestBatchCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingLeaveRequestBatch> FindPendingLeaveRequestBatches(IPendingLeaveRequestBatchStore store, PendingLeaveRequestBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
