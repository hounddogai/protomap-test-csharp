namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy assignments by code.</summary>
public interface IPolicyAssignmentStore
{
    PolicyAssignment? Find(string code);
    void Save(PolicyAssignment item);
    bool Remove(string code);
    List<PolicyAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAssignmentStore(Dictionary<string, PolicyAssignment> items, List<string> order) : IPolicyAssignmentStore
{
    public Dictionary<string, PolicyAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAssignment item)
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

    public List<PolicyAssignment> List()
    {
        var outValue = new List<PolicyAssignment>();
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
public sealed class CachedPolicyAssignmentStore(MemoryPolicyAssignmentStore inner, PolicyAssignmentCache cache, int hits, int misses) : IPolicyAssignmentStore
{
    public MemoryPolicyAssignmentStore Inner { get; set; } = inner;
    public PolicyAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAssignment? Find(string code)
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

    public void Save(PolicyAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy assignments. Empty fields match all.</summary>
public sealed record PolicyAssignmentQuery(string Segment, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAssignment item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAssignment> Apply(List<PolicyAssignment> items)
    {
        var matched = Collections.Filter(items, (PolicyAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy assignment store.</summary>
public static class PolicyAssignmentStore
{
    public static IPolicyAssignmentStore NewMemoryPolicyAssignmentStore()
    {
        return new MemoryPolicyAssignmentStore(new Dictionary<string, PolicyAssignment>(), new List<string>());
    }

    public static IPolicyAssignmentStore NewCachedPolicyAssignmentStore(MemoryPolicyAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAssignmentStore(inner, PolicyAssignmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PolicyAssignment> FindPolicyAssignments(IPolicyAssignmentStore store, PolicyAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
