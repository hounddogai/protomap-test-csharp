namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched certification audits by code.</summary>
public interface IBatchedCertificationAuditStore
{
    BatchedCertificationAudit? Find(string code);
    void Save(BatchedCertificationAudit item);
    bool Remove(string code);
    List<BatchedCertificationAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedCertificationAuditStore(Dictionary<string, BatchedCertificationAudit> items, List<string> order) : IBatchedCertificationAuditStore
{
    public Dictionary<string, BatchedCertificationAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedCertificationAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedCertificationAudit item)
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

    public List<BatchedCertificationAudit> List()
    {
        var outValue = new List<BatchedCertificationAudit>();
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
public sealed class CachedBatchedCertificationAuditStore(MemoryBatchedCertificationAuditStore inner, BatchedCertificationAuditCache cache, int hits, int misses) : IBatchedCertificationAuditStore
{
    public MemoryBatchedCertificationAuditStore Inner { get; set; } = inner;
    public BatchedCertificationAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedCertificationAudit? Find(string code)
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

    public void Save(BatchedCertificationAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedCertificationAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched certification audits. Empty fields match all.</summary>
public sealed record BatchedCertificationAuditQuery(string Segment, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedCertificationAudit item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<BatchedCertificationAudit> Apply(List<BatchedCertificationAudit> items)
    {
        var matched = Collections.Filter(items, (BatchedCertificationAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched certification audit store.</summary>
public static class BatchedCertificationAuditStore
{
    public static IBatchedCertificationAuditStore NewMemoryBatchedCertificationAuditStore()
    {
        return new MemoryBatchedCertificationAuditStore(
            new Dictionary<string, BatchedCertificationAudit>(),
            new List<string>());
    }

    public static IBatchedCertificationAuditStore NewCachedBatchedCertificationAuditStore(MemoryBatchedCertificationAuditStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedCertificationAuditStore(
            inner,
            BatchedCertificationAuditCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<BatchedCertificationAudit> FindBatchedCertificationAudits(IBatchedCertificationAuditStore store, BatchedCertificationAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
