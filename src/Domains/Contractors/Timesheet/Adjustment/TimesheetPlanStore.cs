namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet plans by code.</summary>
public interface ITimesheetPlanStore
{
    TimesheetPlan? Find(string code);
    void Save(TimesheetPlan item);
    bool Remove(string code);
    List<TimesheetPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetPlanStore(Dictionary<string, TimesheetPlan> items, List<string> order) : ITimesheetPlanStore
{
    public Dictionary<string, TimesheetPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetPlan item)
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

    public List<TimesheetPlan> List()
    {
        var outValue = new List<TimesheetPlan>();
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
public sealed class CachedTimesheetPlanStore(MemoryTimesheetPlanStore inner, TimesheetPlanCache cache, int hits, int misses) : ITimesheetPlanStore
{
    public MemoryTimesheetPlanStore Inner { get; set; } = inner;
    public TimesheetPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetPlan? Find(string code)
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

    public void Save(TimesheetPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet plans. Empty fields match all.</summary>
public sealed record TimesheetPlanQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetPlan item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetPlan> Apply(List<TimesheetPlan> items)
    {
        var matched = Collections.Filter(items, (TimesheetPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet plan store.</summary>
public static class TimesheetPlanStore
{
    public static ITimesheetPlanStore NewMemoryTimesheetPlanStore()
    {
        return new MemoryTimesheetPlanStore(new Dictionary<string, TimesheetPlan>(), new List<string>());
    }

    public static ITimesheetPlanStore NewCachedTimesheetPlanStore(MemoryTimesheetPlanStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetPlanStore(inner, TimesheetPlanCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TimesheetPlan> FindTimesheetPlans(ITimesheetPlanStore store, TimesheetPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
