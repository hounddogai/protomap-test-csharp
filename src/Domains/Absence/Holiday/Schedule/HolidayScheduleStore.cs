namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday schedules by code.</summary>
public interface IHolidayScheduleStore
{
    HolidaySchedule? Find(string code);
    void Save(HolidaySchedule item);
    bool Remove(string code);
    List<HolidaySchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayScheduleStore(Dictionary<string, HolidaySchedule> items, List<string> order) : IHolidayScheduleStore
{
    public Dictionary<string, HolidaySchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidaySchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidaySchedule item)
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

    public List<HolidaySchedule> List()
    {
        var outValue = new List<HolidaySchedule>();
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
public sealed class CachedHolidayScheduleStore(MemoryHolidayScheduleStore inner, HolidayScheduleCache cache, int hits, int misses) : IHolidayScheduleStore
{
    public MemoryHolidayScheduleStore Inner { get; set; } = inner;
    public HolidayScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidaySchedule? Find(string code)
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

    public void Save(HolidaySchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidaySchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday schedules. Empty fields match all.</summary>
public sealed record HolidayScheduleQuery(string Channel, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(HolidaySchedule item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidaySchedule> Apply(List<HolidaySchedule> items)
    {
        var matched = Collections.Filter(items, (HolidaySchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday schedule store.</summary>
public static class HolidayScheduleStore
{
    public static IHolidayScheduleStore NewMemoryHolidayScheduleStore()
    {
        return new MemoryHolidayScheduleStore(new Dictionary<string, HolidaySchedule>(), new List<string>());
    }

    public static IHolidayScheduleStore NewCachedHolidayScheduleStore(MemoryHolidayScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayScheduleStore(inner, HolidayScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HolidaySchedule> FindHolidaySchedules(IHolidayScheduleStore store, HolidayScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
