namespace Acme.Workforce.Absence.Policy.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy plans by code.</summary>
public interface IPolicyPlanStore
{
    PolicyPlan? Find(string code);
    void Save(PolicyPlan item);
    bool Remove(string code);
    List<PolicyPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyPlanStore(Dictionary<string, PolicyPlan> items, List<string> order) : IPolicyPlanStore
{
    public Dictionary<string, PolicyPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyPlan item)
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

    public List<PolicyPlan> List()
    {
        var outValue = new List<PolicyPlan>();
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
public sealed class CachedPolicyPlanStore(MemoryPolicyPlanStore inner, PolicyPlanCache cache, int hits, int misses) : IPolicyPlanStore
{
    public MemoryPolicyPlanStore Inner { get; set; } = inner;
    public PolicyPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyPlan? Find(string code)
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

    public void Save(PolicyPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy plans. Empty fields match all.</summary>
public sealed record PolicyPlanQuery(string Category, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyPlan item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<PolicyPlan> Apply(List<PolicyPlan> items)
    {
        var matched = Collections.Filter(items, (PolicyPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy plan store.</summary>
public static class PolicyPlanStore
{
    public static IPolicyPlanStore NewMemoryPolicyPlanStore()
    {
        return new MemoryPolicyPlanStore(new Dictionary<string, PolicyPlan>(), new List<string>());
    }

    public static IPolicyPlanStore NewCachedPolicyPlanStore(MemoryPolicyPlanStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyPlanStore(inner, PolicyPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PolicyPlan> FindPolicyPlans(IPolicyPlanStore store, PolicyPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
