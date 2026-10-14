namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack milestones by code.</summary>
public interface IPolicyAckMilestoneStore
{
    PolicyAckMilestone? Find(string code);
    void Save(PolicyAckMilestone item);
    bool Remove(string code);
    List<PolicyAckMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckMilestoneStore(Dictionary<string, PolicyAckMilestone> items, List<string> order) : IPolicyAckMilestoneStore
{
    public Dictionary<string, PolicyAckMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckMilestone item)
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

    public List<PolicyAckMilestone> List()
    {
        var outValue = new List<PolicyAckMilestone>();
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
public sealed class CachedPolicyAckMilestoneStore(MemoryPolicyAckMilestoneStore inner, PolicyAckMilestoneCache cache, int hits, int misses) : IPolicyAckMilestoneStore
{
    public MemoryPolicyAckMilestoneStore Inner { get; set; } = inner;
    public PolicyAckMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckMilestone? Find(string code)
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

    public void Save(PolicyAckMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack milestones. Empty fields match all.</summary>
public sealed record PolicyAckMilestoneQuery(string Department, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckMilestone item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckMilestone> Apply(List<PolicyAckMilestone> items)
    {
        var matched = Collections.Filter(items, (PolicyAckMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack milestone store.</summary>
public static class PolicyAckMilestoneStore
{
    public static IPolicyAckMilestoneStore NewMemoryPolicyAckMilestoneStore()
    {
        return new MemoryPolicyAckMilestoneStore(new Dictionary<string, PolicyAckMilestone>(), new List<string>());
    }

    public static IPolicyAckMilestoneStore NewCachedPolicyAckMilestoneStore(MemoryPolicyAckMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckMilestoneStore(inner, PolicyAckMilestoneCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PolicyAckMilestone> FindPolicyAckMilestones(IPolicyAckMilestoneStore store, PolicyAckMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
