namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request schedules by code.</summary>
public interface ILeaveRequestScheduleStore
{
    LeaveRequestSchedule? Find(string code);
    void Save(LeaveRequestSchedule item);
    bool Remove(string code);
    List<LeaveRequestSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestScheduleStore(Dictionary<string, LeaveRequestSchedule> items, List<string> order) : ILeaveRequestScheduleStore
{
    public Dictionary<string, LeaveRequestSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestSchedule item)
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

    public List<LeaveRequestSchedule> List()
    {
        var outValue = new List<LeaveRequestSchedule>();
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
public sealed class CachedLeaveRequestScheduleStore(MemoryLeaveRequestScheduleStore inner, LeaveRequestScheduleCache cache, int hits, int misses) : ILeaveRequestScheduleStore
{
    public MemoryLeaveRequestScheduleStore Inner { get; set; } = inner;
    public LeaveRequestScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestSchedule? Find(string code)
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

    public void Save(LeaveRequestSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request schedules. Empty fields match all.</summary>
public sealed record LeaveRequestScheduleQuery(string Region, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestSchedule item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<LeaveRequestSchedule> Apply(List<LeaveRequestSchedule> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request schedule store.</summary>
public static class LeaveRequestScheduleStore
{
    public static ILeaveRequestScheduleStore NewMemoryLeaveRequestScheduleStore()
    {
        return new MemoryLeaveRequestScheduleStore(
            new Dictionary<string, LeaveRequestSchedule>(),
            new List<string>());
    }

    public static ILeaveRequestScheduleStore NewCachedLeaveRequestScheduleStore(MemoryLeaveRequestScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestScheduleStore(inner, LeaveRequestScheduleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestSchedule> FindLeaveRequestSchedules(ILeaveRequestScheduleStore store, LeaveRequestScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
