namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled succession plan audits by code.</summary>
public interface IScheduledSuccessionPlanAuditStore
{
    ScheduledSuccessionPlanAudit? Find(string code);
    void Save(ScheduledSuccessionPlanAudit item);
    bool Remove(string code);
    List<ScheduledSuccessionPlanAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledSuccessionPlanAuditStore(Dictionary<string, ScheduledSuccessionPlanAudit> items, List<string> order) : IScheduledSuccessionPlanAuditStore
{
    public Dictionary<string, ScheduledSuccessionPlanAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledSuccessionPlanAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledSuccessionPlanAudit item)
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

    public List<ScheduledSuccessionPlanAudit> List()
    {
        var outValue = new List<ScheduledSuccessionPlanAudit>();
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
public sealed class CachedScheduledSuccessionPlanAuditStore(MemoryScheduledSuccessionPlanAuditStore inner, ScheduledSuccessionPlanAuditCache cache, int hits, int misses) : IScheduledSuccessionPlanAuditStore
{
    public MemoryScheduledSuccessionPlanAuditStore Inner { get; set; } = inner;
    public ScheduledSuccessionPlanAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledSuccessionPlanAudit? Find(string code)
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

    public void Save(ScheduledSuccessionPlanAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledSuccessionPlanAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled succession plan audits. Empty fields match all.</summary>
public sealed record ScheduledSuccessionPlanAuditQuery(string Country, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledSuccessionPlanAudit item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledSuccessionPlanAudit> Apply(List<ScheduledSuccessionPlanAudit> items)
    {
        var matched = Collections.Filter(items, (ScheduledSuccessionPlanAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled succession plan audit store.</summary>
public static class ScheduledSuccessionPlanAuditStore
{
    public static IScheduledSuccessionPlanAuditStore NewMemoryScheduledSuccessionPlanAuditStore()
    {
        return new MemoryScheduledSuccessionPlanAuditStore(
            new Dictionary<string, ScheduledSuccessionPlanAudit>(),
            new List<string>());
    }

    public static IScheduledSuccessionPlanAuditStore NewCachedScheduledSuccessionPlanAuditStore(MemoryScheduledSuccessionPlanAuditStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledSuccessionPlanAuditStore(
            inner,
            ScheduledSuccessionPlanAuditCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledSuccessionPlanAudit> FindScheduledSuccessionPlanAudits(IScheduledSuccessionPlanAuditStore store, ScheduledSuccessionPlanAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
