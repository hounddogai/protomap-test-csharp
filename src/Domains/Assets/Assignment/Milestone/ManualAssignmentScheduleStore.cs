namespace Acme.Workforce.Assets.Assignment.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual assignment schedules by code.</summary>
public interface IManualAssignmentScheduleStore
{
    ManualAssignmentSchedule? Find(string code);
    void Save(ManualAssignmentSchedule item);
    bool Remove(string code);
    List<ManualAssignmentSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualAssignmentScheduleStore(Dictionary<string, ManualAssignmentSchedule> items, List<string> order) : IManualAssignmentScheduleStore
{
    public Dictionary<string, ManualAssignmentSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualAssignmentSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualAssignmentSchedule item)
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

    public List<ManualAssignmentSchedule> List()
    {
        var outValue = new List<ManualAssignmentSchedule>();
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
public sealed class CachedManualAssignmentScheduleStore(MemoryManualAssignmentScheduleStore inner, ManualAssignmentScheduleCache cache, int hits, int misses) : IManualAssignmentScheduleStore
{
    public MemoryManualAssignmentScheduleStore Inner { get; set; } = inner;
    public ManualAssignmentScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualAssignmentSchedule? Find(string code)
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

    public void Save(ManualAssignmentSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualAssignmentSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual assignment schedules. Empty fields match all.</summary>
public sealed record ManualAssignmentScheduleQuery(string Title, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(ManualAssignmentSchedule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualAssignmentSchedule> Apply(List<ManualAssignmentSchedule> items)
    {
        var matched = Collections.Filter(items, (ManualAssignmentSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual assignment schedule store.</summary>
public static class ManualAssignmentScheduleStore
{
    public static IManualAssignmentScheduleStore NewMemoryManualAssignmentScheduleStore()
    {
        return new MemoryManualAssignmentScheduleStore(
            new Dictionary<string, ManualAssignmentSchedule>(),
            new List<string>());
    }

    public static IManualAssignmentScheduleStore NewCachedManualAssignmentScheduleStore(MemoryManualAssignmentScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedManualAssignmentScheduleStore(
            inner,
            ManualAssignmentScheduleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ManualAssignmentSchedule> FindManualAssignmentSchedules(IManualAssignmentScheduleStore store, ManualAssignmentScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
