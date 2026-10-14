namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched warranty ledgers by code.</summary>
public interface IBatchedWarrantyLedgerStore
{
    BatchedWarrantyLedger? Find(string code);
    void Save(BatchedWarrantyLedger item);
    bool Remove(string code);
    List<BatchedWarrantyLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedWarrantyLedgerStore(Dictionary<string, BatchedWarrantyLedger> items, List<string> order) : IBatchedWarrantyLedgerStore
{
    public Dictionary<string, BatchedWarrantyLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedWarrantyLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedWarrantyLedger item)
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

    public List<BatchedWarrantyLedger> List()
    {
        var outValue = new List<BatchedWarrantyLedger>();
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
public sealed class CachedBatchedWarrantyLedgerStore(MemoryBatchedWarrantyLedgerStore inner, BatchedWarrantyLedgerCache cache, int hits, int misses) : IBatchedWarrantyLedgerStore
{
    public MemoryBatchedWarrantyLedgerStore Inner { get; set; } = inner;
    public BatchedWarrantyLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedWarrantyLedger? Find(string code)
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

    public void Save(BatchedWarrantyLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedWarrantyLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched warranty ledgers. Empty fields match all.</summary>
public sealed record BatchedWarrantyLedgerQuery(string Unit, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedWarrantyLedger item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedWarrantyLedger> Apply(List<BatchedWarrantyLedger> items)
    {
        var matched = Collections.Filter(items, (BatchedWarrantyLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched warranty ledger store.</summary>
public static class BatchedWarrantyLedgerStore
{
    public static IBatchedWarrantyLedgerStore NewMemoryBatchedWarrantyLedgerStore()
    {
        return new MemoryBatchedWarrantyLedgerStore(
            new Dictionary<string, BatchedWarrantyLedger>(),
            new List<string>());
    }

    public static IBatchedWarrantyLedgerStore NewCachedBatchedWarrantyLedgerStore(MemoryBatchedWarrantyLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedWarrantyLedgerStore(
            inner,
            BatchedWarrantyLedgerCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedWarrantyLedger> FindBatchedWarrantyLedgers(IBatchedWarrantyLedgerStore store, BatchedWarrantyLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
