namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched audit cycles by code.</summary>
public interface IBatchedAuditCycleStore
{
    BatchedAuditCycle? Find(string code);
    void Save(BatchedAuditCycle item);
    bool Remove(string code);
    List<BatchedAuditCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAuditCycleStore(Dictionary<string, BatchedAuditCycle> items, List<string> order) : IBatchedAuditCycleStore
{
    public Dictionary<string, BatchedAuditCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAuditCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAuditCycle item)
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

    public List<BatchedAuditCycle> List()
    {
        var outValue = new List<BatchedAuditCycle>();
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
public sealed class CachedBatchedAuditCycleStore(MemoryBatchedAuditCycleStore inner, BatchedAuditCycleCache cache, int hits, int misses) : IBatchedAuditCycleStore
{
    public MemoryBatchedAuditCycleStore Inner { get; set; } = inner;
    public BatchedAuditCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAuditCycle? Find(string code)
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

    public void Save(BatchedAuditCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAuditCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched audit cycles. Empty fields match all.</summary>
public sealed record BatchedAuditCycleQuery(string Country, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAuditCycle item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedAuditCycle> Apply(List<BatchedAuditCycle> items)
    {
        var matched = Collections.Filter(items, (BatchedAuditCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched audit cycle store.</summary>
public static class BatchedAuditCycleStore
{
    public static IBatchedAuditCycleStore NewMemoryBatchedAuditCycleStore()
    {
        return new MemoryBatchedAuditCycleStore(new Dictionary<string, BatchedAuditCycle>(), new List<string>());
    }

    public static IBatchedAuditCycleStore NewCachedBatchedAuditCycleStore(MemoryBatchedAuditCycleStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAuditCycleStore(inner, BatchedAuditCycleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BatchedAuditCycle> FindBatchedAuditCycles(IBatchedAuditCycleStore store, BatchedAuditCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
