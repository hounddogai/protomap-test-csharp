namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet segments by code.</summary>
public interface ITimesheetSegmentStore
{
    TimesheetSegment? Find(string code);
    void Save(TimesheetSegment item);
    bool Remove(string code);
    List<TimesheetSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetSegmentStore(Dictionary<string, TimesheetSegment> items, List<string> order) : ITimesheetSegmentStore
{
    public Dictionary<string, TimesheetSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetSegment item)
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

    public List<TimesheetSegment> List()
    {
        var outValue = new List<TimesheetSegment>();
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
public sealed class CachedTimesheetSegmentStore(MemoryTimesheetSegmentStore inner, TimesheetSegmentCache cache, int hits, int misses) : ITimesheetSegmentStore
{
    public MemoryTimesheetSegmentStore Inner { get; set; } = inner;
    public TimesheetSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetSegment? Find(string code)
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

    public void Save(TimesheetSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet segments. Empty fields match all.</summary>
public sealed record TimesheetSegmentQuery(string Name, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetSegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<TimesheetSegment> Apply(List<TimesheetSegment> items)
    {
        var matched = Collections.Filter(items, (TimesheetSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet segment store.</summary>
public static class TimesheetSegmentStore
{
    public static ITimesheetSegmentStore NewMemoryTimesheetSegmentStore()
    {
        return new MemoryTimesheetSegmentStore(new Dictionary<string, TimesheetSegment>(), new List<string>());
    }

    public static ITimesheetSegmentStore NewCachedTimesheetSegmentStore(MemoryTimesheetSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetSegmentStore(inner, TimesheetSegmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TimesheetSegment> FindTimesheetSegments(ITimesheetSegmentStore store, TimesheetSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
