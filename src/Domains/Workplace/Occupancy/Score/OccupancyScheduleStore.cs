namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy schedules by code.</summary>
public interface IOccupancyScheduleStore
{
    OccupancySchedule? Find(string code);
    void Save(OccupancySchedule item);
    bool Remove(string code);
    List<OccupancySchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyScheduleStore(Dictionary<string, OccupancySchedule> items, List<string> order) : IOccupancyScheduleStore
{
    public Dictionary<string, OccupancySchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancySchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancySchedule item)
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

    public List<OccupancySchedule> List()
    {
        var outValue = new List<OccupancySchedule>();
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
public sealed class CachedOccupancyScheduleStore(MemoryOccupancyScheduleStore inner, OccupancyScheduleCache cache, int hits, int misses) : IOccupancyScheduleStore
{
    public MemoryOccupancyScheduleStore Inner { get; set; } = inner;
    public OccupancyScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancySchedule? Find(string code)
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

    public void Save(OccupancySchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancySchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy schedules. Empty fields match all.</summary>
public sealed record OccupancyScheduleQuery(string Department, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancySchedule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancySchedule> Apply(List<OccupancySchedule> items)
    {
        var matched = Collections.Filter(items, (OccupancySchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy schedule store.</summary>
public static class OccupancyScheduleStore
{
    public static IOccupancyScheduleStore NewMemoryOccupancyScheduleStore()
    {
        return new MemoryOccupancyScheduleStore(new Dictionary<string, OccupancySchedule>(), new List<string>());
    }

    public static IOccupancyScheduleStore NewCachedOccupancyScheduleStore(MemoryOccupancyScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyScheduleStore(inner, OccupancyScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OccupancySchedule> FindOccupancySchedules(IOccupancyScheduleStore store, OccupancyScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
