namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive deduction audits by code.</summary>
public interface IAdaptiveDeductionAuditStore
{
    AdaptiveDeductionAudit? Find(string code);
    void Save(AdaptiveDeductionAudit item);
    bool Remove(string code);
    List<AdaptiveDeductionAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveDeductionAuditStore(Dictionary<string, AdaptiveDeductionAudit> items, List<string> order) : IAdaptiveDeductionAuditStore
{
    public Dictionary<string, AdaptiveDeductionAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveDeductionAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveDeductionAudit item)
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

    public List<AdaptiveDeductionAudit> List()
    {
        var outValue = new List<AdaptiveDeductionAudit>();
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
public sealed class CachedAdaptiveDeductionAuditStore(MemoryAdaptiveDeductionAuditStore inner, AdaptiveDeductionAuditCache cache, int hits, int misses) : IAdaptiveDeductionAuditStore
{
    public MemoryAdaptiveDeductionAuditStore Inner { get; set; } = inner;
    public AdaptiveDeductionAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveDeductionAudit? Find(string code)
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

    public void Save(AdaptiveDeductionAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveDeductionAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive deduction audits. Empty fields match all.</summary>
public sealed record AdaptiveDeductionAuditQuery(string Label, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveDeductionAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveDeductionAudit> Apply(List<AdaptiveDeductionAudit> items)
    {
        var matched = Collections.Filter(items, (AdaptiveDeductionAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive deduction audit store.</summary>
public static class AdaptiveDeductionAuditStore
{
    public static IAdaptiveDeductionAuditStore NewMemoryAdaptiveDeductionAuditStore()
    {
        return new MemoryAdaptiveDeductionAuditStore(
            new Dictionary<string, AdaptiveDeductionAudit>(),
            new List<string>());
    }

    public static IAdaptiveDeductionAuditStore NewCachedAdaptiveDeductionAuditStore(MemoryAdaptiveDeductionAuditStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveDeductionAuditStore(
            inner,
            AdaptiveDeductionAuditCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveDeductionAudit> FindAdaptiveDeductionAudits(IAdaptiveDeductionAuditStore store, AdaptiveDeductionAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
