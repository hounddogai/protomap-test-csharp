namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched span ledgers by code.</summary>
public interface IBatchedSpanLedgerStore
{
    BatchedSpanLedger? Find(string code);
    void Save(BatchedSpanLedger item);
    bool Remove(string code);
    List<BatchedSpanLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedSpanLedgerStore(Dictionary<string, BatchedSpanLedger> items, List<string> order) : IBatchedSpanLedgerStore
{
    public Dictionary<string, BatchedSpanLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedSpanLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedSpanLedger item)
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

    public List<BatchedSpanLedger> List()
    {
        var outValue = new List<BatchedSpanLedger>();
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
public sealed class CachedBatchedSpanLedgerStore(MemoryBatchedSpanLedgerStore inner, BatchedSpanLedgerCache cache, int hits, int misses) : IBatchedSpanLedgerStore
{
    public MemoryBatchedSpanLedgerStore Inner { get; set; } = inner;
    public BatchedSpanLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedSpanLedger? Find(string code)
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

    public void Save(BatchedSpanLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedSpanLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched span ledgers. Empty fields match all.</summary>
public sealed record BatchedSpanLedgerQuery(string Source, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedSpanLedger item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<BatchedSpanLedger> Apply(List<BatchedSpanLedger> items)
    {
        var matched = Collections.Filter(items, (BatchedSpanLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched span ledger store.</summary>
public static class BatchedSpanLedgerStore
{
    public static IBatchedSpanLedgerStore NewMemoryBatchedSpanLedgerStore()
    {
        return new MemoryBatchedSpanLedgerStore(new Dictionary<string, BatchedSpanLedger>(), new List<string>());
    }

    public static IBatchedSpanLedgerStore NewCachedBatchedSpanLedgerStore(MemoryBatchedSpanLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedSpanLedgerStore(inner, BatchedSpanLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BatchedSpanLedger> FindBatchedSpanLedgers(IBatchedSpanLedgerStore store, BatchedSpanLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
