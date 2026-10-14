namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan schedules by code.</summary>
public interface IReturnPlanScheduleStore
{
    ReturnPlanSchedule? Find(string code);
    void Save(ReturnPlanSchedule item);
    bool Remove(string code);
    List<ReturnPlanSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanScheduleStore(Dictionary<string, ReturnPlanSchedule> items, List<string> order) : IReturnPlanScheduleStore
{
    public Dictionary<string, ReturnPlanSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanSchedule item)
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

    public List<ReturnPlanSchedule> List()
    {
        var outValue = new List<ReturnPlanSchedule>();
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
public sealed class CachedReturnPlanScheduleStore(MemoryReturnPlanScheduleStore inner, ReturnPlanScheduleCache cache, int hits, int misses) : IReturnPlanScheduleStore
{
    public MemoryReturnPlanScheduleStore Inner { get; set; } = inner;
    public ReturnPlanScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanSchedule? Find(string code)
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

    public void Save(ReturnPlanSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan schedules. Empty fields match all.</summary>
public sealed record ReturnPlanScheduleQuery(string Channel, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanSchedule item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanSchedule> Apply(List<ReturnPlanSchedule> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan schedule store.</summary>
public static class ReturnPlanScheduleStore
{
    public static IReturnPlanScheduleStore NewMemoryReturnPlanScheduleStore()
    {
        return new MemoryReturnPlanScheduleStore(new Dictionary<string, ReturnPlanSchedule>(), new List<string>());
    }

    public static IReturnPlanScheduleStore NewCachedReturnPlanScheduleStore(MemoryReturnPlanScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanScheduleStore(inner, ReturnPlanScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanSchedule> FindReturnPlanSchedules(IReturnPlanScheduleStore store, ReturnPlanScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
