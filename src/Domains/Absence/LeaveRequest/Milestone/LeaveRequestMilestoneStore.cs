namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request milestones by code.</summary>
public interface ILeaveRequestMilestoneStore
{
    LeaveRequestMilestone? Find(string code);
    void Save(LeaveRequestMilestone item);
    bool Remove(string code);
    List<LeaveRequestMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestMilestoneStore(Dictionary<string, LeaveRequestMilestone> items, List<string> order) : ILeaveRequestMilestoneStore
{
    public Dictionary<string, LeaveRequestMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestMilestone item)
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

    public List<LeaveRequestMilestone> List()
    {
        var outValue = new List<LeaveRequestMilestone>();
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
public sealed class CachedLeaveRequestMilestoneStore(MemoryLeaveRequestMilestoneStore inner, LeaveRequestMilestoneCache cache, int hits, int misses) : ILeaveRequestMilestoneStore
{
    public MemoryLeaveRequestMilestoneStore Inner { get; set; } = inner;
    public LeaveRequestMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestMilestone? Find(string code)
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

    public void Save(LeaveRequestMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request milestones. Empty fields match all.</summary>
public sealed record LeaveRequestMilestoneQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestMilestone item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestMilestone> Apply(List<LeaveRequestMilestone> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request milestone store.</summary>
public static class LeaveRequestMilestoneStore
{
    public static ILeaveRequestMilestoneStore NewMemoryLeaveRequestMilestoneStore()
    {
        return new MemoryLeaveRequestMilestoneStore(
            new Dictionary<string, LeaveRequestMilestone>(),
            new List<string>());
    }

    public static ILeaveRequestMilestoneStore NewCachedLeaveRequestMilestoneStore(MemoryLeaveRequestMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestMilestoneStore(inner, LeaveRequestMilestoneCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestMilestone> FindLeaveRequestMilestones(ILeaveRequestMilestoneStore store, LeaveRequestMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
