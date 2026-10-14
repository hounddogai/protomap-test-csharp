namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched scenario audits by code.</summary>
public interface IBatchedScenarioAuditStore
{
    BatchedScenarioAudit? Find(string code);
    void Save(BatchedScenarioAudit item);
    bool Remove(string code);
    List<BatchedScenarioAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedScenarioAuditStore(Dictionary<string, BatchedScenarioAudit> items, List<string> order) : IBatchedScenarioAuditStore
{
    public Dictionary<string, BatchedScenarioAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedScenarioAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedScenarioAudit item)
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

    public List<BatchedScenarioAudit> List()
    {
        var outValue = new List<BatchedScenarioAudit>();
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
public sealed class CachedBatchedScenarioAuditStore(MemoryBatchedScenarioAuditStore inner, BatchedScenarioAuditCache cache, int hits, int misses) : IBatchedScenarioAuditStore
{
    public MemoryBatchedScenarioAuditStore Inner { get; set; } = inner;
    public BatchedScenarioAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedScenarioAudit? Find(string code)
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

    public void Save(BatchedScenarioAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedScenarioAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched scenario audits. Empty fields match all.</summary>
public sealed record BatchedScenarioAuditQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedScenarioAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<BatchedScenarioAudit> Apply(List<BatchedScenarioAudit> items)
    {
        var matched = Collections.Filter(items, (BatchedScenarioAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched scenario audit store.</summary>
public static class BatchedScenarioAuditStore
{
    public static IBatchedScenarioAuditStore NewMemoryBatchedScenarioAuditStore()
    {
        return new MemoryBatchedScenarioAuditStore(
            new Dictionary<string, BatchedScenarioAudit>(),
            new List<string>());
    }

    public static IBatchedScenarioAuditStore NewCachedBatchedScenarioAuditStore(MemoryBatchedScenarioAuditStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedScenarioAuditStore(inner, BatchedScenarioAuditCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<BatchedScenarioAudit> FindBatchedScenarioAudits(IBatchedScenarioAuditStore store, BatchedScenarioAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
