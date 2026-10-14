namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet requests by code.</summary>
public interface ITimesheetRequestStore
{
    TimesheetRequest? Find(string code);
    void Save(TimesheetRequest item);
    bool Remove(string code);
    List<TimesheetRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetRequestStore(Dictionary<string, TimesheetRequest> items, List<string> order) : ITimesheetRequestStore
{
    public Dictionary<string, TimesheetRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetRequest item)
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

    public List<TimesheetRequest> List()
    {
        var outValue = new List<TimesheetRequest>();
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
public sealed class CachedTimesheetRequestStore(MemoryTimesheetRequestStore inner, TimesheetRequestCache cache, int hits, int misses) : ITimesheetRequestStore
{
    public MemoryTimesheetRequestStore Inner { get; set; } = inner;
    public TimesheetRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetRequest? Find(string code)
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

    public void Save(TimesheetRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet requests. Empty fields match all.</summary>
public sealed record TimesheetRequestQuery(string Source, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetRequest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<TimesheetRequest> Apply(List<TimesheetRequest> items)
    {
        var matched = Collections.Filter(items, (TimesheetRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet request store.</summary>
public static class TimesheetRequestStore
{
    public static ITimesheetRequestStore NewMemoryTimesheetRequestStore()
    {
        return new MemoryTimesheetRequestStore(new Dictionary<string, TimesheetRequest>(), new List<string>());
    }

    public static ITimesheetRequestStore NewCachedTimesheetRequestStore(MemoryTimesheetRequestStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetRequestStore(inner, TimesheetRequestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TimesheetRequest> FindTimesheetRequests(ITimesheetRequestStore store, TimesheetRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
