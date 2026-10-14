namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy holiday schedules by code.</summary>
public interface ILegacyHolidayScheduleStore
{
    LegacyHolidaySchedule? Find(string code);
    void Save(LegacyHolidaySchedule item);
    bool Remove(string code);
    List<LegacyHolidaySchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyHolidayScheduleStore(Dictionary<string, LegacyHolidaySchedule> items, List<string> order) : ILegacyHolidayScheduleStore
{
    public Dictionary<string, LegacyHolidaySchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyHolidaySchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyHolidaySchedule item)
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

    public List<LegacyHolidaySchedule> List()
    {
        var outValue = new List<LegacyHolidaySchedule>();
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
public sealed class CachedLegacyHolidayScheduleStore(MemoryLegacyHolidayScheduleStore inner, LegacyHolidayScheduleCache cache, int hits, int misses) : ILegacyHolidayScheduleStore
{
    public MemoryLegacyHolidayScheduleStore Inner { get; set; } = inner;
    public LegacyHolidayScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyHolidaySchedule? Find(string code)
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

    public void Save(LegacyHolidaySchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyHolidaySchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy holiday schedules. Empty fields match all.</summary>
public sealed record LegacyHolidayScheduleQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyHolidaySchedule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyHolidaySchedule> Apply(List<LegacyHolidaySchedule> items)
    {
        var matched = Collections.Filter(items, (LegacyHolidaySchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy holiday schedule store.</summary>
public static class LegacyHolidayScheduleStore
{
    public static ILegacyHolidayScheduleStore NewMemoryLegacyHolidayScheduleStore()
    {
        return new MemoryLegacyHolidayScheduleStore(
            new Dictionary<string, LegacyHolidaySchedule>(),
            new List<string>());
    }

    public static ILegacyHolidayScheduleStore NewCachedLegacyHolidayScheduleStore(MemoryLegacyHolidayScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyHolidayScheduleStore(inner, LegacyHolidayScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LegacyHolidaySchedule> FindLegacyHolidaySchedules(ILegacyHolidayScheduleStore store, LegacyHolidayScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
