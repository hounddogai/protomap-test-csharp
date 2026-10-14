namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet adjustments by code.</summary>
public interface ITimesheetAdjustmentStore
{
    TimesheetAdjustment? Find(string code);
    void Save(TimesheetAdjustment item);
    bool Remove(string code);
    List<TimesheetAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetAdjustmentStore(Dictionary<string, TimesheetAdjustment> items, List<string> order) : ITimesheetAdjustmentStore
{
    public Dictionary<string, TimesheetAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetAdjustment item)
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

    public List<TimesheetAdjustment> List()
    {
        var outValue = new List<TimesheetAdjustment>();
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
public sealed class CachedTimesheetAdjustmentStore(MemoryTimesheetAdjustmentStore inner, TimesheetAdjustmentCache cache, int hits, int misses) : ITimesheetAdjustmentStore
{
    public MemoryTimesheetAdjustmentStore Inner { get; set; } = inner;
    public TimesheetAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetAdjustment? Find(string code)
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

    public void Save(TimesheetAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet adjustments. Empty fields match all.</summary>
public sealed record TimesheetAdjustmentQuery(string Source, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetAdjustment item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<TimesheetAdjustment> Apply(List<TimesheetAdjustment> items)
    {
        var matched = Collections.Filter(items, (TimesheetAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet adjustment store.</summary>
public static class TimesheetAdjustmentStore
{
    public static ITimesheetAdjustmentStore NewMemoryTimesheetAdjustmentStore()
    {
        return new MemoryTimesheetAdjustmentStore(new Dictionary<string, TimesheetAdjustment>(), new List<string>());
    }

    public static ITimesheetAdjustmentStore NewCachedTimesheetAdjustmentStore(MemoryTimesheetAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetAdjustmentStore(inner, TimesheetAdjustmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TimesheetAdjustment> FindTimesheetAdjustments(ITimesheetAdjustmentStore store, TimesheetAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
