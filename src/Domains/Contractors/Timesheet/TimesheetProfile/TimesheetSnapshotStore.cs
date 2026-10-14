namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet snapshots by code.</summary>
public interface ITimesheetSnapshotStore
{
    TimesheetSnapshot? Find(string code);
    void Save(TimesheetSnapshot item);
    bool Remove(string code);
    List<TimesheetSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetSnapshotStore(Dictionary<string, TimesheetSnapshot> items, List<string> order) : ITimesheetSnapshotStore
{
    public Dictionary<string, TimesheetSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetSnapshot item)
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

    public List<TimesheetSnapshot> List()
    {
        var outValue = new List<TimesheetSnapshot>();
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
public sealed class CachedTimesheetSnapshotStore(MemoryTimesheetSnapshotStore inner, TimesheetSnapshotCache cache, int hits, int misses) : ITimesheetSnapshotStore
{
    public MemoryTimesheetSnapshotStore Inner { get; set; } = inner;
    public TimesheetSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetSnapshot? Find(string code)
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

    public void Save(TimesheetSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet snapshots. Empty fields match all.</summary>
public sealed record TimesheetSnapshotQuery(string Title, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetSnapshot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetSnapshot> Apply(List<TimesheetSnapshot> items)
    {
        var matched = Collections.Filter(items, (TimesheetSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet snapshot store.</summary>
public static class TimesheetSnapshotStore
{
    public static ITimesheetSnapshotStore NewMemoryTimesheetSnapshotStore()
    {
        return new MemoryTimesheetSnapshotStore(new Dictionary<string, TimesheetSnapshot>(), new List<string>());
    }

    public static ITimesheetSnapshotStore NewCachedTimesheetSnapshotStore(MemoryTimesheetSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetSnapshotStore(inner, TimesheetSnapshotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<TimesheetSnapshot> FindTimesheetSnapshots(ITimesheetSnapshotStore store, TimesheetSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
