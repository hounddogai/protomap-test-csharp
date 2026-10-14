namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched pool audits by code.</summary>
public interface IBatchedPoolAuditStore
{
    BatchedPoolAudit? Find(string code);
    void Save(BatchedPoolAudit item);
    bool Remove(string code);
    List<BatchedPoolAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedPoolAuditStore(Dictionary<string, BatchedPoolAudit> items, List<string> order) : IBatchedPoolAuditStore
{
    public Dictionary<string, BatchedPoolAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedPoolAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedPoolAudit item)
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

    public List<BatchedPoolAudit> List()
    {
        var outValue = new List<BatchedPoolAudit>();
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
public sealed class CachedBatchedPoolAuditStore(MemoryBatchedPoolAuditStore inner, BatchedPoolAuditCache cache, int hits, int misses) : IBatchedPoolAuditStore
{
    public MemoryBatchedPoolAuditStore Inner { get; set; } = inner;
    public BatchedPoolAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedPoolAudit? Find(string code)
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

    public void Save(BatchedPoolAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedPoolAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched pool audits. Empty fields match all.</summary>
public sealed record BatchedPoolAuditQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedPoolAudit item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedPoolAudit> Apply(List<BatchedPoolAudit> items)
    {
        var matched = Collections.Filter(items, (BatchedPoolAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched pool audit store.</summary>
public static class BatchedPoolAuditStore
{
    public static IBatchedPoolAuditStore NewMemoryBatchedPoolAuditStore()
    {
        return new MemoryBatchedPoolAuditStore(new Dictionary<string, BatchedPoolAudit>(), new List<string>());
    }

    public static IBatchedPoolAuditStore NewCachedBatchedPoolAuditStore(MemoryBatchedPoolAuditStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedPoolAuditStore(inner, BatchedPoolAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BatchedPoolAudit> FindBatchedPoolAudits(IBatchedPoolAuditStore store, BatchedPoolAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
