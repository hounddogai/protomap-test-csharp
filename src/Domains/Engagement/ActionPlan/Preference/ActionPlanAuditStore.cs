namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan audits by code.</summary>
public interface IActionPlanAuditStore
{
    ActionPlanAudit? Find(string code);
    void Save(ActionPlanAudit item);
    bool Remove(string code);
    List<ActionPlanAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanAuditStore(Dictionary<string, ActionPlanAudit> items, List<string> order) : IActionPlanAuditStore
{
    public Dictionary<string, ActionPlanAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanAudit item)
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

    public List<ActionPlanAudit> List()
    {
        var outValue = new List<ActionPlanAudit>();
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
public sealed class CachedActionPlanAuditStore(MemoryActionPlanAuditStore inner, ActionPlanAuditCache cache, int hits, int misses) : IActionPlanAuditStore
{
    public MemoryActionPlanAuditStore Inner { get; set; } = inner;
    public ActionPlanAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanAudit? Find(string code)
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

    public void Save(ActionPlanAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan audits. Empty fields match all.</summary>
public sealed record ActionPlanAuditQuery(string Country, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanAudit item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanAudit> Apply(List<ActionPlanAudit> items)
    {
        var matched = Collections.Filter(items, (ActionPlanAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan audit store.</summary>
public static class ActionPlanAuditStore
{
    public static IActionPlanAuditStore NewMemoryActionPlanAuditStore()
    {
        return new MemoryActionPlanAuditStore(new Dictionary<string, ActionPlanAudit>(), new List<string>());
    }

    public static IActionPlanAuditStore NewCachedActionPlanAuditStore(MemoryActionPlanAuditStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanAuditStore(inner, ActionPlanAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ActionPlanAudit> FindActionPlanAudits(IActionPlanAuditStore store, ActionPlanAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
