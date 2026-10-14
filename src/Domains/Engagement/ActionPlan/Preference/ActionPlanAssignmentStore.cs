namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan assignments by code.</summary>
public interface IActionPlanAssignmentStore
{
    ActionPlanAssignment? Find(string code);
    void Save(ActionPlanAssignment item);
    bool Remove(string code);
    List<ActionPlanAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanAssignmentStore(Dictionary<string, ActionPlanAssignment> items, List<string> order) : IActionPlanAssignmentStore
{
    public Dictionary<string, ActionPlanAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanAssignment item)
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

    public List<ActionPlanAssignment> List()
    {
        var outValue = new List<ActionPlanAssignment>();
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
public sealed class CachedActionPlanAssignmentStore(MemoryActionPlanAssignmentStore inner, ActionPlanAssignmentCache cache, int hits, int misses) : IActionPlanAssignmentStore
{
    public MemoryActionPlanAssignmentStore Inner { get; set; } = inner;
    public ActionPlanAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanAssignment? Find(string code)
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

    public void Save(ActionPlanAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan assignments. Empty fields match all.</summary>
public sealed record ActionPlanAssignmentQuery(string Segment, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanAssignment item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanAssignment> Apply(List<ActionPlanAssignment> items)
    {
        var matched = Collections.Filter(items, (ActionPlanAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan assignment store.</summary>
public static class ActionPlanAssignmentStore
{
    public static IActionPlanAssignmentStore NewMemoryActionPlanAssignmentStore()
    {
        return new MemoryActionPlanAssignmentStore(
            new Dictionary<string, ActionPlanAssignment>(),
            new List<string>());
    }

    public static IActionPlanAssignmentStore NewCachedActionPlanAssignmentStore(MemoryActionPlanAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanAssignmentStore(inner, ActionPlanAssignmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ActionPlanAssignment> FindActionPlanAssignments(IActionPlanAssignmentStore store, ActionPlanAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
