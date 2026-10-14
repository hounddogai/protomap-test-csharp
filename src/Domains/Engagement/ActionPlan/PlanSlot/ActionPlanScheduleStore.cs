namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan schedules by code.</summary>
public interface IActionPlanScheduleStore
{
    ActionPlanSchedule? Find(string code);
    void Save(ActionPlanSchedule item);
    bool Remove(string code);
    List<ActionPlanSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanScheduleStore(Dictionary<string, ActionPlanSchedule> items, List<string> order) : IActionPlanScheduleStore
{
    public Dictionary<string, ActionPlanSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanSchedule item)
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

    public List<ActionPlanSchedule> List()
    {
        var outValue = new List<ActionPlanSchedule>();
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
public sealed class CachedActionPlanScheduleStore(MemoryActionPlanScheduleStore inner, ActionPlanScheduleCache cache, int hits, int misses) : IActionPlanScheduleStore
{
    public MemoryActionPlanScheduleStore Inner { get; set; } = inner;
    public ActionPlanScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanSchedule? Find(string code)
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

    public void Save(ActionPlanSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan schedules. Empty fields match all.</summary>
public sealed record ActionPlanScheduleQuery(string Unit, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanSchedule item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<ActionPlanSchedule> Apply(List<ActionPlanSchedule> items)
    {
        var matched = Collections.Filter(items, (ActionPlanSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan schedule store.</summary>
public static class ActionPlanScheduleStore
{
    public static IActionPlanScheduleStore NewMemoryActionPlanScheduleStore()
    {
        return new MemoryActionPlanScheduleStore(new Dictionary<string, ActionPlanSchedule>(), new List<string>());
    }

    public static IActionPlanScheduleStore NewCachedActionPlanScheduleStore(MemoryActionPlanScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanScheduleStore(inner, ActionPlanScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ActionPlanSchedule> FindActionPlanSchedules(IActionPlanScheduleStore store, ActionPlanScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
