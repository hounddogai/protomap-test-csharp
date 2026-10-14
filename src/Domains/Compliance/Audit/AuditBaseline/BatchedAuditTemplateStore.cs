namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched audit templates by code.</summary>
public interface IBatchedAuditTemplateStore
{
    BatchedAuditTemplate? Find(string code);
    void Save(BatchedAuditTemplate item);
    bool Remove(string code);
    List<BatchedAuditTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedAuditTemplateStore(Dictionary<string, BatchedAuditTemplate> items, List<string> order) : IBatchedAuditTemplateStore
{
    public Dictionary<string, BatchedAuditTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedAuditTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedAuditTemplate item)
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

    public List<BatchedAuditTemplate> List()
    {
        var outValue = new List<BatchedAuditTemplate>();
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
public sealed class CachedBatchedAuditTemplateStore(MemoryBatchedAuditTemplateStore inner, BatchedAuditTemplateCache cache, int hits, int misses) : IBatchedAuditTemplateStore
{
    public MemoryBatchedAuditTemplateStore Inner { get; set; } = inner;
    public BatchedAuditTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedAuditTemplate? Find(string code)
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

    public void Save(BatchedAuditTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedAuditTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched audit templates. Empty fields match all.</summary>
public sealed record BatchedAuditTemplateQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedAuditTemplate item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedAuditTemplate> Apply(List<BatchedAuditTemplate> items)
    {
        var matched = Collections.Filter(items, (BatchedAuditTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched audit template store.</summary>
public static class BatchedAuditTemplateStore
{
    public static IBatchedAuditTemplateStore NewMemoryBatchedAuditTemplateStore()
    {
        return new MemoryBatchedAuditTemplateStore(
            new Dictionary<string, BatchedAuditTemplate>(),
            new List<string>());
    }

    public static IBatchedAuditTemplateStore NewCachedBatchedAuditTemplateStore(MemoryBatchedAuditTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedAuditTemplateStore(inner, BatchedAuditTemplateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BatchedAuditTemplate> FindBatchedAuditTemplates(IBatchedAuditTemplateStore store, BatchedAuditTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
