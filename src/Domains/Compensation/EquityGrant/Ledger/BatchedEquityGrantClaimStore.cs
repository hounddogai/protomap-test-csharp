namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched equity grant claims by code.</summary>
public interface IBatchedEquityGrantClaimStore
{
    BatchedEquityGrantClaim? Find(string code);
    void Save(BatchedEquityGrantClaim item);
    bool Remove(string code);
    List<BatchedEquityGrantClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedEquityGrantClaimStore(Dictionary<string, BatchedEquityGrantClaim> items, List<string> order) : IBatchedEquityGrantClaimStore
{
    public Dictionary<string, BatchedEquityGrantClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedEquityGrantClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedEquityGrantClaim item)
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

    public List<BatchedEquityGrantClaim> List()
    {
        var outValue = new List<BatchedEquityGrantClaim>();
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
public sealed class CachedBatchedEquityGrantClaimStore(MemoryBatchedEquityGrantClaimStore inner, BatchedEquityGrantClaimCache cache, int hits, int misses) : IBatchedEquityGrantClaimStore
{
    public MemoryBatchedEquityGrantClaimStore Inner { get; set; } = inner;
    public BatchedEquityGrantClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedEquityGrantClaim? Find(string code)
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

    public void Save(BatchedEquityGrantClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedEquityGrantClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched equity grant claims. Empty fields match all.</summary>
public sealed record BatchedEquityGrantClaimQuery(string Category, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedEquityGrantClaim item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedEquityGrantClaim> Apply(List<BatchedEquityGrantClaim> items)
    {
        var matched = Collections.Filter(items, (BatchedEquityGrantClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched equity grant claim store.</summary>
public static class BatchedEquityGrantClaimStore
{
    public static IBatchedEquityGrantClaimStore NewMemoryBatchedEquityGrantClaimStore()
    {
        return new MemoryBatchedEquityGrantClaimStore(
            new Dictionary<string, BatchedEquityGrantClaim>(),
            new List<string>());
    }

    public static IBatchedEquityGrantClaimStore NewCachedBatchedEquityGrantClaimStore(MemoryBatchedEquityGrantClaimStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedEquityGrantClaimStore(
            inner,
            BatchedEquityGrantClaimCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<BatchedEquityGrantClaim> FindBatchedEquityGrantClaims(IBatchedEquityGrantClaimStore store, BatchedEquityGrantClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
