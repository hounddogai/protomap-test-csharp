namespace Acme.Workforce.Absence.Holiday.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback holiday schedules by code.</summary>
public interface IFallbackHolidayScheduleStore
{
    FallbackHolidaySchedule? Find(string code);
    void Save(FallbackHolidaySchedule item);
    bool Remove(string code);
    List<FallbackHolidaySchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackHolidayScheduleStore(Dictionary<string, FallbackHolidaySchedule> items, List<string> order) : IFallbackHolidayScheduleStore
{
    public Dictionary<string, FallbackHolidaySchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackHolidaySchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackHolidaySchedule item)
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

    public List<FallbackHolidaySchedule> List()
    {
        var outValue = new List<FallbackHolidaySchedule>();
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
public sealed class CachedFallbackHolidayScheduleStore(MemoryFallbackHolidayScheduleStore inner, FallbackHolidayScheduleCache cache, int hits, int misses) : IFallbackHolidayScheduleStore
{
    public MemoryFallbackHolidayScheduleStore Inner { get; set; } = inner;
    public FallbackHolidayScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackHolidaySchedule? Find(string code)
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

    public void Save(FallbackHolidaySchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackHolidaySchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback holiday schedules. Empty fields match all.</summary>
public sealed record FallbackHolidayScheduleQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackHolidaySchedule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackHolidaySchedule> Apply(List<FallbackHolidaySchedule> items)
    {
        var matched = Collections.Filter(items, (FallbackHolidaySchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback holiday schedule store.</summary>
public static class FallbackHolidayScheduleStore
{
    public static IFallbackHolidayScheduleStore NewMemoryFallbackHolidayScheduleStore()
    {
        return new MemoryFallbackHolidayScheduleStore(
            new Dictionary<string, FallbackHolidaySchedule>(),
            new List<string>());
    }

    public static IFallbackHolidayScheduleStore NewCachedFallbackHolidayScheduleStore(MemoryFallbackHolidayScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackHolidayScheduleStore(
            inner,
            FallbackHolidayScheduleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackHolidaySchedule> FindFallbackHolidaySchedules(IFallbackHolidayScheduleStore store, FallbackHolidayScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
