namespace Acme.Workforce.Contractors.Timesheet.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet records by code.</summary>
public interface ITimesheetRecordStore
{
    TimesheetRecord? Find(string code);
    void Save(TimesheetRecord item);
    bool Remove(string code);
    List<TimesheetRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetRecordStore(Dictionary<string, TimesheetRecord> items, List<string> order) : ITimesheetRecordStore
{
    public Dictionary<string, TimesheetRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetRecord item)
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

    public List<TimesheetRecord> List()
    {
        var outValue = new List<TimesheetRecord>();
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
public sealed class CachedTimesheetRecordStore(MemoryTimesheetRecordStore inner, TimesheetRecordCache cache, int hits, int misses) : ITimesheetRecordStore
{
    public MemoryTimesheetRecordStore Inner { get; set; } = inner;
    public TimesheetRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetRecord? Find(string code)
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

    public void Save(TimesheetRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet records. Empty fields match all.</summary>
public sealed record TimesheetRecordQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetRecord item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetRecord> Apply(List<TimesheetRecord> items)
    {
        var matched = Collections.Filter(items, (TimesheetRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet record store.</summary>
public static class TimesheetRecordStore
{
    public static ITimesheetRecordStore NewMemoryTimesheetRecordStore()
    {
        return new MemoryTimesheetRecordStore(new Dictionary<string, TimesheetRecord>(), new List<string>());
    }

    public static ITimesheetRecordStore NewCachedTimesheetRecordStore(MemoryTimesheetRecordStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetRecordStore(inner, TimesheetRecordCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TimesheetRecord> FindTimesheetRecords(ITimesheetRecordStore store, TimesheetRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
