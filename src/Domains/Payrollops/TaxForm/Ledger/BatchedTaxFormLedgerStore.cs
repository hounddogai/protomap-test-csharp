namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched tax form ledgers by code.</summary>
public interface IBatchedTaxFormLedgerStore
{
    BatchedTaxFormLedger? Find(string code);
    void Save(BatchedTaxFormLedger item);
    bool Remove(string code);
    List<BatchedTaxFormLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedTaxFormLedgerStore(Dictionary<string, BatchedTaxFormLedger> items, List<string> order) : IBatchedTaxFormLedgerStore
{
    public Dictionary<string, BatchedTaxFormLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedTaxFormLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedTaxFormLedger item)
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

    public List<BatchedTaxFormLedger> List()
    {
        var outValue = new List<BatchedTaxFormLedger>();
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
public sealed class CachedBatchedTaxFormLedgerStore(MemoryBatchedTaxFormLedgerStore inner, BatchedTaxFormLedgerCache cache, int hits, int misses) : IBatchedTaxFormLedgerStore
{
    public MemoryBatchedTaxFormLedgerStore Inner { get; set; } = inner;
    public BatchedTaxFormLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedTaxFormLedger? Find(string code)
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

    public void Save(BatchedTaxFormLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedTaxFormLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched tax form ledgers. Empty fields match all.</summary>
public sealed record BatchedTaxFormLedgerQuery(string Unit, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedTaxFormLedger item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedTaxFormLedger> Apply(List<BatchedTaxFormLedger> items)
    {
        var matched = Collections.Filter(items, (BatchedTaxFormLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched tax form ledger store.</summary>
public static class BatchedTaxFormLedgerStore
{
    public static IBatchedTaxFormLedgerStore NewMemoryBatchedTaxFormLedgerStore()
    {
        return new MemoryBatchedTaxFormLedgerStore(
            new Dictionary<string, BatchedTaxFormLedger>(),
            new List<string>());
    }

    public static IBatchedTaxFormLedgerStore NewCachedBatchedTaxFormLedgerStore(MemoryBatchedTaxFormLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedTaxFormLedgerStore(inner, BatchedTaxFormLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BatchedTaxFormLedger> FindBatchedTaxFormLedgers(IBatchedTaxFormLedgerStore store, BatchedTaxFormLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
