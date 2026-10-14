namespace Acme.Workforce.Engagement.ActionPlan.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default action plan policies by code.</summary>
public interface IDefaultActionPlanPolicyStore
{
    DefaultActionPlanPolicy? Find(string code);
    void Save(DefaultActionPlanPolicy item);
    bool Remove(string code);
    List<DefaultActionPlanPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultActionPlanPolicyStore(Dictionary<string, DefaultActionPlanPolicy> items, List<string> order) : IDefaultActionPlanPolicyStore
{
    public Dictionary<string, DefaultActionPlanPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultActionPlanPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultActionPlanPolicy item)
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

    public List<DefaultActionPlanPolicy> List()
    {
        var outValue = new List<DefaultActionPlanPolicy>();
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
public sealed class CachedDefaultActionPlanPolicyStore(MemoryDefaultActionPlanPolicyStore inner, DefaultActionPlanPolicyCache cache, int hits, int misses) : IDefaultActionPlanPolicyStore
{
    public MemoryDefaultActionPlanPolicyStore Inner { get; set; } = inner;
    public DefaultActionPlanPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultActionPlanPolicy? Find(string code)
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

    public void Save(DefaultActionPlanPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultActionPlanPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default action plan policies. Empty fields match all.</summary>
public sealed record DefaultActionPlanPolicyQuery(string Category, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultActionPlanPolicy item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultActionPlanPolicy> Apply(List<DefaultActionPlanPolicy> items)
    {
        var matched = Collections.Filter(items, (DefaultActionPlanPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default action plan policy store.</summary>
public static class DefaultActionPlanPolicyStore
{
    public static IDefaultActionPlanPolicyStore NewMemoryDefaultActionPlanPolicyStore()
    {
        return new MemoryDefaultActionPlanPolicyStore(
            new Dictionary<string, DefaultActionPlanPolicy>(),
            new List<string>());
    }

    public static IDefaultActionPlanPolicyStore NewCachedDefaultActionPlanPolicyStore(MemoryDefaultActionPlanPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultActionPlanPolicyStore(
            inner,
            DefaultActionPlanPolicyCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultActionPlanPolicy> FindDefaultActionPlanPolicies(IDefaultActionPlanPolicyStore store, DefaultActionPlanPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
