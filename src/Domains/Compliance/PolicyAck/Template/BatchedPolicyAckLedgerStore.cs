namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched policy ack ledgers by code.</summary>
public interface IBatchedPolicyAckLedgerStore
{
    BatchedPolicyAckLedger? Find(string code);
    void Save(BatchedPolicyAckLedger item);
    bool Remove(string code);
    List<BatchedPolicyAckLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedPolicyAckLedgerStore(Dictionary<string, BatchedPolicyAckLedger> items, List<string> order) : IBatchedPolicyAckLedgerStore
{
    public Dictionary<string, BatchedPolicyAckLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedPolicyAckLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedPolicyAckLedger item)
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

    public List<BatchedPolicyAckLedger> List()
    {
        var outValue = new List<BatchedPolicyAckLedger>();
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
public sealed class CachedBatchedPolicyAckLedgerStore(MemoryBatchedPolicyAckLedgerStore inner, BatchedPolicyAckLedgerCache cache, int hits, int misses) : IBatchedPolicyAckLedgerStore
{
    public MemoryBatchedPolicyAckLedgerStore Inner { get; set; } = inner;
    public BatchedPolicyAckLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedPolicyAckLedger? Find(string code)
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

    public void Save(BatchedPolicyAckLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedPolicyAckLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched policy ack ledgers. Empty fields match all.</summary>
public sealed record BatchedPolicyAckLedgerQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedPolicyAckLedger item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedPolicyAckLedger> Apply(List<BatchedPolicyAckLedger> items)
    {
        var matched = Collections.Filter(items, (BatchedPolicyAckLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched policy ack ledger store.</summary>
public static class BatchedPolicyAckLedgerStore
{
    public static IBatchedPolicyAckLedgerStore NewMemoryBatchedPolicyAckLedgerStore()
    {
        return new MemoryBatchedPolicyAckLedgerStore(
            new Dictionary<string, BatchedPolicyAckLedger>(),
            new List<string>());
    }

    public static IBatchedPolicyAckLedgerStore NewCachedBatchedPolicyAckLedgerStore(MemoryBatchedPolicyAckLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedPolicyAckLedgerStore(
            inner,
            BatchedPolicyAckLedgerCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<BatchedPolicyAckLedger> FindBatchedPolicyAckLedgers(IBatchedPolicyAckLedgerStore store, BatchedPolicyAckLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
