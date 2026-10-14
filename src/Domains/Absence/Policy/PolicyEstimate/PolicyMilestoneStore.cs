namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy milestones by code.</summary>
public interface IPolicyMilestoneStore
{
    PolicyMilestone? Find(string code);
    void Save(PolicyMilestone item);
    bool Remove(string code);
    List<PolicyMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyMilestoneStore(Dictionary<string, PolicyMilestone> items, List<string> order) : IPolicyMilestoneStore
{
    public Dictionary<string, PolicyMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyMilestone item)
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

    public List<PolicyMilestone> List()
    {
        var outValue = new List<PolicyMilestone>();
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
public sealed class CachedPolicyMilestoneStore(MemoryPolicyMilestoneStore inner, PolicyMilestoneCache cache, int hits, int misses) : IPolicyMilestoneStore
{
    public MemoryPolicyMilestoneStore Inner { get; set; } = inner;
    public PolicyMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyMilestone? Find(string code)
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

    public void Save(PolicyMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy milestones. Empty fields match all.</summary>
public sealed record PolicyMilestoneQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyMilestone item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyMilestone> Apply(List<PolicyMilestone> items)
    {
        var matched = Collections.Filter(items, (PolicyMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy milestone store.</summary>
public static class PolicyMilestoneStore
{
    public static IPolicyMilestoneStore NewMemoryPolicyMilestoneStore()
    {
        return new MemoryPolicyMilestoneStore(new Dictionary<string, PolicyMilestone>(), new List<string>());
    }

    public static IPolicyMilestoneStore NewCachedPolicyMilestoneStore(MemoryPolicyMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyMilestoneStore(inner, PolicyMilestoneCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PolicyMilestone> FindPolicyMilestones(IPolicyMilestoneStore store, PolicyMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
