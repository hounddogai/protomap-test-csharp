namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched booking audits by code.</summary>
public interface IBatchedBookingAuditStore
{
    BatchedBookingAudit? Find(string code);
    void Save(BatchedBookingAudit item);
    bool Remove(string code);
    List<BatchedBookingAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedBookingAuditStore(Dictionary<string, BatchedBookingAudit> items, List<string> order) : IBatchedBookingAuditStore
{
    public Dictionary<string, BatchedBookingAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedBookingAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedBookingAudit item)
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

    public List<BatchedBookingAudit> List()
    {
        var outValue = new List<BatchedBookingAudit>();
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
public sealed class CachedBatchedBookingAuditStore(MemoryBatchedBookingAuditStore inner, BatchedBookingAuditCache cache, int hits, int misses) : IBatchedBookingAuditStore
{
    public MemoryBatchedBookingAuditStore Inner { get; set; } = inner;
    public BatchedBookingAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedBookingAudit? Find(string code)
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

    public void Save(BatchedBookingAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedBookingAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched booking audits. Empty fields match all.</summary>
public sealed record BatchedBookingAuditQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedBookingAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedBookingAudit> Apply(List<BatchedBookingAudit> items)
    {
        var matched = Collections.Filter(items, (BatchedBookingAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched booking audit store.</summary>
public static class BatchedBookingAuditStore
{
    public static IBatchedBookingAuditStore NewMemoryBatchedBookingAuditStore()
    {
        return new MemoryBatchedBookingAuditStore(new Dictionary<string, BatchedBookingAudit>(), new List<string>());
    }

    public static IBatchedBookingAuditStore NewCachedBatchedBookingAuditStore(MemoryBatchedBookingAuditStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedBookingAuditStore(inner, BatchedBookingAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BatchedBookingAudit> FindBatchedBookingAudits(IBatchedBookingAuditStore store, BatchedBookingAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
