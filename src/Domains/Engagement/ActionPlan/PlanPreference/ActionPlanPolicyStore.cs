namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan policies by code.</summary>
public interface IActionPlanPolicyStore
{
    ActionPlanPolicy? Find(string code);
    void Save(ActionPlanPolicy item);
    bool Remove(string code);
    List<ActionPlanPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanPolicyStore(Dictionary<string, ActionPlanPolicy> items, List<string> order) : IActionPlanPolicyStore
{
    public Dictionary<string, ActionPlanPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanPolicy item)
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

    public List<ActionPlanPolicy> List()
    {
        var outValue = new List<ActionPlanPolicy>();
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
public sealed class CachedActionPlanPolicyStore(MemoryActionPlanPolicyStore inner, ActionPlanPolicyCache cache, int hits, int misses) : IActionPlanPolicyStore
{
    public MemoryActionPlanPolicyStore Inner { get; set; } = inner;
    public ActionPlanPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanPolicy? Find(string code)
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

    public void Save(ActionPlanPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan policies. Empty fields match all.</summary>
public sealed record ActionPlanPolicyQuery(string Department, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanPolicy item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<ActionPlanPolicy> Apply(List<ActionPlanPolicy> items)
    {
        var matched = Collections.Filter(items, (ActionPlanPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan policy store.</summary>
public static class ActionPlanPolicyStore
{
    public static IActionPlanPolicyStore NewMemoryActionPlanPolicyStore()
    {
        return new MemoryActionPlanPolicyStore(new Dictionary<string, ActionPlanPolicy>(), new List<string>());
    }

    public static IActionPlanPolicyStore NewCachedActionPlanPolicyStore(MemoryActionPlanPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanPolicyStore(inner, ActionPlanPolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ActionPlanPolicy> FindActionPlanPolicies(IActionPlanPolicyStore store, ActionPlanPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
