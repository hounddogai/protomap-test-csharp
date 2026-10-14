namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched leave request items by code.</summary>
public interface IBatchedLeaveRequestItemStore
{
    BatchedLeaveRequestItem? Find(string code);
    void Save(BatchedLeaveRequestItem item);
    bool Remove(string code);
    List<BatchedLeaveRequestItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedLeaveRequestItemStore(Dictionary<string, BatchedLeaveRequestItem> items, List<string> order) : IBatchedLeaveRequestItemStore
{
    public Dictionary<string, BatchedLeaveRequestItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedLeaveRequestItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedLeaveRequestItem item)
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

    public List<BatchedLeaveRequestItem> List()
    {
        var outValue = new List<BatchedLeaveRequestItem>();
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
public sealed class CachedBatchedLeaveRequestItemStore(MemoryBatchedLeaveRequestItemStore inner, BatchedLeaveRequestItemCache cache, int hits, int misses) : IBatchedLeaveRequestItemStore
{
    public MemoryBatchedLeaveRequestItemStore Inner { get; set; } = inner;
    public BatchedLeaveRequestItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedLeaveRequestItem? Find(string code)
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

    public void Save(BatchedLeaveRequestItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedLeaveRequestItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched leave request items. Empty fields match all.</summary>
public sealed record BatchedLeaveRequestItemQuery(string Category, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedLeaveRequestItem item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<BatchedLeaveRequestItem> Apply(List<BatchedLeaveRequestItem> items)
    {
        var matched = Collections.Filter(items, (BatchedLeaveRequestItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched leave request item store.</summary>
public static class BatchedLeaveRequestItemStore
{
    public static IBatchedLeaveRequestItemStore NewMemoryBatchedLeaveRequestItemStore()
    {
        return new MemoryBatchedLeaveRequestItemStore(
            new Dictionary<string, BatchedLeaveRequestItem>(),
            new List<string>());
    }

    public static IBatchedLeaveRequestItemStore NewCachedBatchedLeaveRequestItemStore(MemoryBatchedLeaveRequestItemStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedLeaveRequestItemStore(
            inner,
            BatchedLeaveRequestItemCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<BatchedLeaveRequestItem> FindBatchedLeaveRequestItems(IBatchedLeaveRequestItemStore store, BatchedLeaveRequestItemQuery query)
    {
        return query.Apply(store.List());
    }
}
