namespace Acme.Workforce.Assets.Assignment.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment milestones by code.</summary>
public interface IAssignmentMilestoneStore
{
    AssignmentMilestone? Find(string code);
    void Save(AssignmentMilestone item);
    bool Remove(string code);
    List<AssignmentMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentMilestoneStore(Dictionary<string, AssignmentMilestone> items, List<string> order) : IAssignmentMilestoneStore
{
    public Dictionary<string, AssignmentMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentMilestone item)
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

    public List<AssignmentMilestone> List()
    {
        var outValue = new List<AssignmentMilestone>();
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
public sealed class CachedAssignmentMilestoneStore(MemoryAssignmentMilestoneStore inner, AssignmentMilestoneCache cache, int hits, int misses) : IAssignmentMilestoneStore
{
    public MemoryAssignmentMilestoneStore Inner { get; set; } = inner;
    public AssignmentMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentMilestone? Find(string code)
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

    public void Save(AssignmentMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment milestones. Empty fields match all.</summary>
public sealed record AssignmentMilestoneQuery(string Channel, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentMilestone item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentMilestone> Apply(List<AssignmentMilestone> items)
    {
        var matched = Collections.Filter(items, (AssignmentMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment milestone store.</summary>
public static class AssignmentMilestoneStore
{
    public static IAssignmentMilestoneStore NewMemoryAssignmentMilestoneStore()
    {
        return new MemoryAssignmentMilestoneStore(new Dictionary<string, AssignmentMilestone>(), new List<string>());
    }

    public static IAssignmentMilestoneStore NewCachedAssignmentMilestoneStore(MemoryAssignmentMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentMilestoneStore(inner, AssignmentMilestoneCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AssignmentMilestone> FindAssignmentMilestones(IAssignmentMilestoneStore store, AssignmentMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
