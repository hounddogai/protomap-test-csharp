namespace Acme.Workforce.Contractors.Timesheet.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet allocations by code.</summary>
public interface ITimesheetAllocationStore
{
    TimesheetAllocation? Find(string code);
    void Save(TimesheetAllocation item);
    bool Remove(string code);
    List<TimesheetAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetAllocationStore(Dictionary<string, TimesheetAllocation> items, List<string> order) : ITimesheetAllocationStore
{
    public Dictionary<string, TimesheetAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetAllocation item)
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

    public List<TimesheetAllocation> List()
    {
        var outValue = new List<TimesheetAllocation>();
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
public sealed class CachedTimesheetAllocationStore(MemoryTimesheetAllocationStore inner, TimesheetAllocationCache cache, int hits, int misses) : ITimesheetAllocationStore
{
    public MemoryTimesheetAllocationStore Inner { get; set; } = inner;
    public TimesheetAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetAllocation? Find(string code)
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

    public void Save(TimesheetAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet allocations. Empty fields match all.</summary>
public sealed record TimesheetAllocationQuery(string Segment, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetAllocation item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetAllocation> Apply(List<TimesheetAllocation> items)
    {
        var matched = Collections.Filter(items, (TimesheetAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet allocation store.</summary>
public static class TimesheetAllocationStore
{
    public static ITimesheetAllocationStore NewMemoryTimesheetAllocationStore()
    {
        return new MemoryTimesheetAllocationStore(new Dictionary<string, TimesheetAllocation>(), new List<string>());
    }

    public static ITimesheetAllocationStore NewCachedTimesheetAllocationStore(MemoryTimesheetAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetAllocationStore(inner, TimesheetAllocationCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TimesheetAllocation> FindTimesheetAllocations(ITimesheetAllocationStore store, TimesheetAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
