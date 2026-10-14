namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched booking ledgers by code.</summary>
public interface IBatchedBookingLedgerStore
{
    BatchedBookingLedger? Find(string code);
    void Save(BatchedBookingLedger item);
    bool Remove(string code);
    List<BatchedBookingLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedBookingLedgerStore(Dictionary<string, BatchedBookingLedger> items, List<string> order) : IBatchedBookingLedgerStore
{
    public Dictionary<string, BatchedBookingLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedBookingLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedBookingLedger item)
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

    public List<BatchedBookingLedger> List()
    {
        var outValue = new List<BatchedBookingLedger>();
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
public sealed class CachedBatchedBookingLedgerStore(MemoryBatchedBookingLedgerStore inner, BatchedBookingLedgerCache cache, int hits, int misses) : IBatchedBookingLedgerStore
{
    public MemoryBatchedBookingLedgerStore Inner { get; set; } = inner;
    public BatchedBookingLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedBookingLedger? Find(string code)
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

    public void Save(BatchedBookingLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedBookingLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched booking ledgers. Empty fields match all.</summary>
public sealed record BatchedBookingLedgerQuery(string Segment, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedBookingLedger item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedBookingLedger> Apply(List<BatchedBookingLedger> items)
    {
        var matched = Collections.Filter(items, (BatchedBookingLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched booking ledger store.</summary>
public static class BatchedBookingLedgerStore
{
    public static IBatchedBookingLedgerStore NewMemoryBatchedBookingLedgerStore()
    {
        return new MemoryBatchedBookingLedgerStore(
            new Dictionary<string, BatchedBookingLedger>(),
            new List<string>());
    }

    public static IBatchedBookingLedgerStore NewCachedBatchedBookingLedgerStore(MemoryBatchedBookingLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedBookingLedgerStore(inner, BatchedBookingLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BatchedBookingLedger> FindBatchedBookingLedgers(IBatchedBookingLedgerStore store, BatchedBookingLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
