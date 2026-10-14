namespace Acme.Workforce.Engagement.ActionPlan.PlanEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling action plan schedules by code.</summary>
public interface IRollingActionPlanScheduleStore
{
    RollingActionPlanSchedule? Find(string code);
    void Save(RollingActionPlanSchedule item);
    bool Remove(string code);
    List<RollingActionPlanSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingActionPlanScheduleStore(Dictionary<string, RollingActionPlanSchedule> items, List<string> order) : IRollingActionPlanScheduleStore
{
    public Dictionary<string, RollingActionPlanSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingActionPlanSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingActionPlanSchedule item)
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

    public List<RollingActionPlanSchedule> List()
    {
        var outValue = new List<RollingActionPlanSchedule>();
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
public sealed class CachedRollingActionPlanScheduleStore(MemoryRollingActionPlanScheduleStore inner, RollingActionPlanScheduleCache cache, int hits, int misses) : IRollingActionPlanScheduleStore
{
    public MemoryRollingActionPlanScheduleStore Inner { get; set; } = inner;
    public RollingActionPlanScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingActionPlanSchedule? Find(string code)
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

    public void Save(RollingActionPlanSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingActionPlanSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling action plan schedules. Empty fields match all.</summary>
public sealed record RollingActionPlanScheduleQuery(string Source, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(RollingActionPlanSchedule item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingActionPlanSchedule> Apply(List<RollingActionPlanSchedule> items)
    {
        var matched = Collections.Filter(items, (RollingActionPlanSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling action plan schedule store.</summary>
public static class RollingActionPlanScheduleStore
{
    public static IRollingActionPlanScheduleStore NewMemoryRollingActionPlanScheduleStore()
    {
        return new MemoryRollingActionPlanScheduleStore(
            new Dictionary<string, RollingActionPlanSchedule>(),
            new List<string>());
    }

    public static IRollingActionPlanScheduleStore NewCachedRollingActionPlanScheduleStore(MemoryRollingActionPlanScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedRollingActionPlanScheduleStore(
            inner,
            RollingActionPlanScheduleCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RollingActionPlanSchedule> FindRollingActionPlanSchedules(IRollingActionPlanScheduleStore store, RollingActionPlanScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
