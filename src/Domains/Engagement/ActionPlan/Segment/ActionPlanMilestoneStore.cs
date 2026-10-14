namespace Acme.Workforce.Engagement.ActionPlan.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan milestones by code.</summary>
public interface IActionPlanMilestoneStore
{
    ActionPlanMilestone? Find(string code);
    void Save(ActionPlanMilestone item);
    bool Remove(string code);
    List<ActionPlanMilestone> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanMilestoneStore(Dictionary<string, ActionPlanMilestone> items, List<string> order) : IActionPlanMilestoneStore
{
    public Dictionary<string, ActionPlanMilestone> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanMilestone? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanMilestone item)
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

    public List<ActionPlanMilestone> List()
    {
        var outValue = new List<ActionPlanMilestone>();
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
public sealed class CachedActionPlanMilestoneStore(MemoryActionPlanMilestoneStore inner, ActionPlanMilestoneCache cache, int hits, int misses) : IActionPlanMilestoneStore
{
    public MemoryActionPlanMilestoneStore Inner { get; set; } = inner;
    public ActionPlanMilestoneCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanMilestone? Find(string code)
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

    public void Save(ActionPlanMilestone item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanMilestone> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan milestones. Empty fields match all.</summary>
public sealed record ActionPlanMilestoneQuery(string Source, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanMilestone item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<ActionPlanMilestone> Apply(List<ActionPlanMilestone> items)
    {
        var matched = Collections.Filter(items, (ActionPlanMilestone candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan milestone store.</summary>
public static class ActionPlanMilestoneStore
{
    public static IActionPlanMilestoneStore NewMemoryActionPlanMilestoneStore()
    {
        return new MemoryActionPlanMilestoneStore(new Dictionary<string, ActionPlanMilestone>(), new List<string>());
    }

    public static IActionPlanMilestoneStore NewCachedActionPlanMilestoneStore(MemoryActionPlanMilestoneStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanMilestoneStore(inner, ActionPlanMilestoneCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ActionPlanMilestone> FindActionPlanMilestones(IActionPlanMilestoneStore store, ActionPlanMilestoneQuery query)
    {
        return query.Apply(store.List());
    }
}
