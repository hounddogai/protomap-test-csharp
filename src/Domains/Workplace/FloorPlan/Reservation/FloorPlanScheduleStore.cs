namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan schedules by code.</summary>
public interface IFloorPlanScheduleStore
{
    FloorPlanSchedule? Find(string code);
    void Save(FloorPlanSchedule item);
    bool Remove(string code);
    List<FloorPlanSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanScheduleStore(Dictionary<string, FloorPlanSchedule> items, List<string> order) : IFloorPlanScheduleStore
{
    public Dictionary<string, FloorPlanSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanSchedule item)
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

    public List<FloorPlanSchedule> List()
    {
        var outValue = new List<FloorPlanSchedule>();
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
public sealed class CachedFloorPlanScheduleStore(MemoryFloorPlanScheduleStore inner, FloorPlanScheduleCache cache, int hits, int misses) : IFloorPlanScheduleStore
{
    public MemoryFloorPlanScheduleStore Inner { get; set; } = inner;
    public FloorPlanScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanSchedule? Find(string code)
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

    public void Save(FloorPlanSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan schedules. Empty fields match all.</summary>
public sealed record FloorPlanScheduleQuery(string Channel, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanSchedule item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanSchedule> Apply(List<FloorPlanSchedule> items)
    {
        var matched = Collections.Filter(items, (FloorPlanSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan schedule store.</summary>
public static class FloorPlanScheduleStore
{
    public static IFloorPlanScheduleStore NewMemoryFloorPlanScheduleStore()
    {
        return new MemoryFloorPlanScheduleStore(new Dictionary<string, FloorPlanSchedule>(), new List<string>());
    }

    public static IFloorPlanScheduleStore NewCachedFloorPlanScheduleStore(MemoryFloorPlanScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanScheduleStore(inner, FloorPlanScheduleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FloorPlanSchedule> FindFloorPlanSchedules(IFloorPlanScheduleStore store, FloorPlanScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
