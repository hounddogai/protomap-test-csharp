namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet cycles by code.</summary>
public interface ITimesheetCycleStore
{
    TimesheetCycle? Find(string code);
    void Save(TimesheetCycle item);
    bool Remove(string code);
    List<TimesheetCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetCycleStore(Dictionary<string, TimesheetCycle> items, List<string> order) : ITimesheetCycleStore
{
    public Dictionary<string, TimesheetCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetCycle item)
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

    public List<TimesheetCycle> List()
    {
        var outValue = new List<TimesheetCycle>();
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
public sealed class CachedTimesheetCycleStore(MemoryTimesheetCycleStore inner, TimesheetCycleCache cache, int hits, int misses) : ITimesheetCycleStore
{
    public MemoryTimesheetCycleStore Inner { get; set; } = inner;
    public TimesheetCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetCycle? Find(string code)
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

    public void Save(TimesheetCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet cycles. Empty fields match all.</summary>
public sealed record TimesheetCycleQuery(string Channel, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetCycle item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetCycle> Apply(List<TimesheetCycle> items)
    {
        var matched = Collections.Filter(items, (TimesheetCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet cycle store.</summary>
public static class TimesheetCycleStore
{
    public static ITimesheetCycleStore NewMemoryTimesheetCycleStore()
    {
        return new MemoryTimesheetCycleStore(new Dictionary<string, TimesheetCycle>(), new List<string>());
    }

    public static ITimesheetCycleStore NewCachedTimesheetCycleStore(MemoryTimesheetCycleStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetCycleStore(inner, TimesheetCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TimesheetCycle> FindTimesheetCycles(ITimesheetCycleStore store, TimesheetCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
