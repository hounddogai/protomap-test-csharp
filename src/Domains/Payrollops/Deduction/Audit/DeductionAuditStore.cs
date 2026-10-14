namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction audits by code.</summary>
public interface IDeductionAuditStore
{
    DeductionAudit? Find(string code);
    void Save(DeductionAudit item);
    bool Remove(string code);
    List<DeductionAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionAuditStore(Dictionary<string, DeductionAudit> items, List<string> order) : IDeductionAuditStore
{
    public Dictionary<string, DeductionAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionAudit item)
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

    public List<DeductionAudit> List()
    {
        var outValue = new List<DeductionAudit>();
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
public sealed class CachedDeductionAuditStore(MemoryDeductionAuditStore inner, DeductionAuditCache cache, int hits, int misses) : IDeductionAuditStore
{
    public MemoryDeductionAuditStore Inner { get; set; } = inner;
    public DeductionAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionAudit? Find(string code)
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

    public void Save(DeductionAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction audits. Empty fields match all.</summary>
public sealed record DeductionAuditQuery(string Region, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionAudit item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<DeductionAudit> Apply(List<DeductionAudit> items)
    {
        var matched = Collections.Filter(items, (DeductionAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction audit store.</summary>
public static class DeductionAuditStore
{
    public static IDeductionAuditStore NewMemoryDeductionAuditStore()
    {
        return new MemoryDeductionAuditStore(new Dictionary<string, DeductionAudit>(), new List<string>());
    }

    public static IDeductionAuditStore NewCachedDeductionAuditStore(MemoryDeductionAuditStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionAuditStore(inner, DeductionAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DeductionAudit> FindDeductionAudits(IDeductionAuditStore store, DeductionAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
