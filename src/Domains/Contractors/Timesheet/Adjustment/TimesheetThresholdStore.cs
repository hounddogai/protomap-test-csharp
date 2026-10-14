namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet thresholds by code.</summary>
public interface ITimesheetThresholdStore
{
    TimesheetThreshold? Find(string code);
    void Save(TimesheetThreshold item);
    bool Remove(string code);
    List<TimesheetThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetThresholdStore(Dictionary<string, TimesheetThreshold> items, List<string> order) : ITimesheetThresholdStore
{
    public Dictionary<string, TimesheetThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetThreshold item)
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

    public List<TimesheetThreshold> List()
    {
        var outValue = new List<TimesheetThreshold>();
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
public sealed class CachedTimesheetThresholdStore(MemoryTimesheetThresholdStore inner, TimesheetThresholdCache cache, int hits, int misses) : ITimesheetThresholdStore
{
    public MemoryTimesheetThresholdStore Inner { get; set; } = inner;
    public TimesheetThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetThreshold? Find(string code)
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

    public void Save(TimesheetThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet thresholds. Empty fields match all.</summary>
public sealed record TimesheetThresholdQuery(string Source, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetThreshold item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetThreshold> Apply(List<TimesheetThreshold> items)
    {
        var matched = Collections.Filter(items, (TimesheetThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet threshold store.</summary>
public static class TimesheetThresholdStore
{
    public static ITimesheetThresholdStore NewMemoryTimesheetThresholdStore()
    {
        return new MemoryTimesheetThresholdStore(new Dictionary<string, TimesheetThreshold>(), new List<string>());
    }

    public static ITimesheetThresholdStore NewCachedTimesheetThresholdStore(MemoryTimesheetThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetThresholdStore(inner, TimesheetThresholdCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TimesheetThreshold> FindTimesheetThresholds(ITimesheetThresholdStore store, TimesheetThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
