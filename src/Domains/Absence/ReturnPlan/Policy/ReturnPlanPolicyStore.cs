namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan policies by code.</summary>
public interface IReturnPlanPolicyStore
{
    ReturnPlanPolicy? Find(string code);
    void Save(ReturnPlanPolicy item);
    bool Remove(string code);
    List<ReturnPlanPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanPolicyStore(Dictionary<string, ReturnPlanPolicy> items, List<string> order) : IReturnPlanPolicyStore
{
    public Dictionary<string, ReturnPlanPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanPolicy item)
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

    public List<ReturnPlanPolicy> List()
    {
        var outValue = new List<ReturnPlanPolicy>();
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
public sealed class CachedReturnPlanPolicyStore(MemoryReturnPlanPolicyStore inner, ReturnPlanPolicyCache cache, int hits, int misses) : IReturnPlanPolicyStore
{
    public MemoryReturnPlanPolicyStore Inner { get; set; } = inner;
    public ReturnPlanPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanPolicy? Find(string code)
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

    public void Save(ReturnPlanPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan policies. Empty fields match all.</summary>
public sealed record ReturnPlanPolicyQuery(string Channel, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanPolicy item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanPolicy> Apply(List<ReturnPlanPolicy> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan policy store.</summary>
public static class ReturnPlanPolicyStore
{
    public static IReturnPlanPolicyStore NewMemoryReturnPlanPolicyStore()
    {
        return new MemoryReturnPlanPolicyStore(new Dictionary<string, ReturnPlanPolicy>(), new List<string>());
    }

    public static IReturnPlanPolicyStore NewCachedReturnPlanPolicyStore(MemoryReturnPlanPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanPolicyStore(inner, ReturnPlanPolicyCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanPolicy> FindReturnPlanPolicies(IReturnPlanPolicyStore store, ReturnPlanPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
