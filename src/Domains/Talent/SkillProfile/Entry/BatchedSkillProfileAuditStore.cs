namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched skill profile audits by code.</summary>
public interface IBatchedSkillProfileAuditStore
{
    BatchedSkillProfileAudit? Find(string code);
    void Save(BatchedSkillProfileAudit item);
    bool Remove(string code);
    List<BatchedSkillProfileAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedSkillProfileAuditStore(Dictionary<string, BatchedSkillProfileAudit> items, List<string> order) : IBatchedSkillProfileAuditStore
{
    public Dictionary<string, BatchedSkillProfileAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedSkillProfileAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedSkillProfileAudit item)
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

    public List<BatchedSkillProfileAudit> List()
    {
        var outValue = new List<BatchedSkillProfileAudit>();
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
public sealed class CachedBatchedSkillProfileAuditStore(MemoryBatchedSkillProfileAuditStore inner, BatchedSkillProfileAuditCache cache, int hits, int misses) : IBatchedSkillProfileAuditStore
{
    public MemoryBatchedSkillProfileAuditStore Inner { get; set; } = inner;
    public BatchedSkillProfileAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedSkillProfileAudit? Find(string code)
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

    public void Save(BatchedSkillProfileAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedSkillProfileAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched skill profile audits. Empty fields match all.</summary>
public sealed record BatchedSkillProfileAuditQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedSkillProfileAudit item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BatchedSkillProfileAudit> Apply(List<BatchedSkillProfileAudit> items)
    {
        var matched = Collections.Filter(items, (BatchedSkillProfileAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched skill profile audit store.</summary>
public static class BatchedSkillProfileAuditStore
{
    public static IBatchedSkillProfileAuditStore NewMemoryBatchedSkillProfileAuditStore()
    {
        return new MemoryBatchedSkillProfileAuditStore(
            new Dictionary<string, BatchedSkillProfileAudit>(),
            new List<string>());
    }

    public static IBatchedSkillProfileAuditStore NewCachedBatchedSkillProfileAuditStore(MemoryBatchedSkillProfileAuditStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedSkillProfileAuditStore(
            inner,
            BatchedSkillProfileAuditCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<BatchedSkillProfileAudit> FindBatchedSkillProfileAudits(IBatchedSkillProfileAuditStore store, BatchedSkillProfileAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
