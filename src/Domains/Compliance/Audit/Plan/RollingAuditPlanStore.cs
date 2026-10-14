namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling audit plans by code.</summary>
public interface IRollingAuditPlanStore
{
    RollingAuditPlan? Find(string code);
    void Save(RollingAuditPlan item);
    bool Remove(string code);
    List<RollingAuditPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingAuditPlanStore(Dictionary<string, RollingAuditPlan> items, List<string> order) : IRollingAuditPlanStore
{
    public Dictionary<string, RollingAuditPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingAuditPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingAuditPlan item)
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

    public List<RollingAuditPlan> List()
    {
        var outValue = new List<RollingAuditPlan>();
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
public sealed class CachedRollingAuditPlanStore(MemoryRollingAuditPlanStore inner, RollingAuditPlanCache cache, int hits, int misses) : IRollingAuditPlanStore
{
    public MemoryRollingAuditPlanStore Inner { get; set; } = inner;
    public RollingAuditPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingAuditPlan? Find(string code)
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

    public void Save(RollingAuditPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingAuditPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling audit plans. Empty fields match all.</summary>
public sealed record RollingAuditPlanQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(RollingAuditPlan item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingAuditPlan> Apply(List<RollingAuditPlan> items)
    {
        var matched = Collections.Filter(items, (RollingAuditPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling audit plan store.</summary>
public static class RollingAuditPlanStore
{
    public static IRollingAuditPlanStore NewMemoryRollingAuditPlanStore()
    {
        return new MemoryRollingAuditPlanStore(new Dictionary<string, RollingAuditPlan>(), new List<string>());
    }

    public static IRollingAuditPlanStore NewCachedRollingAuditPlanStore(MemoryRollingAuditPlanStore inner, int ttl, IClock clock)
    {
        return new CachedRollingAuditPlanStore(inner, RollingAuditPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RollingAuditPlan> FindRollingAuditPlans(IRollingAuditPlanStore store, RollingAuditPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
