namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched audit summaries by code.</summary>
public interface IBatchedAuditSummaryStore
{
    BatchedAuditSummary? Find(string code);
    void Save(BatchedAuditSummary item);
    bool Remove(string code);
    List<BatchedAuditSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAuditSummaryStore(Dictionary<string, BatchedAuditSummary> items, List<string> order) : IBatchedAuditSummaryStore
{
    public Dictionary<string, BatchedAuditSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAuditSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAuditSummary item)
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

    public List<BatchedAuditSummary> List()
    {
        var outValue = new List<BatchedAuditSummary>();
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
public sealed class CachedBatchedAuditSummaryStore(MemoryBatchedAuditSummaryStore inner, BatchedAuditSummaryCache cache, int hits, int misses) : IBatchedAuditSummaryStore
{
    public MemoryBatchedAuditSummaryStore Inner { get; set; } = inner;
    public BatchedAuditSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAuditSummary? Find(string code)
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

    public void Save(BatchedAuditSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAuditSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched audit summaries. Empty fields match all.</summary>
public sealed record BatchedAuditSummaryQuery(string Channel, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAuditSummary item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedAuditSummary> Apply(List<BatchedAuditSummary> items)
    {
        var matched = Collections.Filter(items, (BatchedAuditSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched audit summary store.</summary>
public static class BatchedAuditSummaryStore
{
    public static IBatchedAuditSummaryStore NewMemoryBatchedAuditSummaryStore()
    {
        return new MemoryBatchedAuditSummaryStore(new Dictionary<string, BatchedAuditSummary>(), new List<string>());
    }

    public static IBatchedAuditSummaryStore NewCachedBatchedAuditSummaryStore(MemoryBatchedAuditSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAuditSummaryStore(inner, BatchedAuditSummaryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BatchedAuditSummary> FindBatchedAuditSummaries(IBatchedAuditSummaryStore store, BatchedAuditSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
