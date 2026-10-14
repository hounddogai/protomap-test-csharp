namespace Acme.Workforce.Payrollops.PayRun.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient pay run schedules by code.</summary>
public interface ILenientPayRunScheduleStore
{
    LenientPayRunSchedule? Find(string code);
    void Save(LenientPayRunSchedule item);
    bool Remove(string code);
    List<LenientPayRunSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPayRunScheduleStore(Dictionary<string, LenientPayRunSchedule> items, List<string> order) : ILenientPayRunScheduleStore
{
    public Dictionary<string, LenientPayRunSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPayRunSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPayRunSchedule item)
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

    public List<LenientPayRunSchedule> List()
    {
        var outValue = new List<LenientPayRunSchedule>();
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
public sealed class CachedLenientPayRunScheduleStore(MemoryLenientPayRunScheduleStore inner, LenientPayRunScheduleCache cache, int hits, int misses) : ILenientPayRunScheduleStore
{
    public MemoryLenientPayRunScheduleStore Inner { get; set; } = inner;
    public LenientPayRunScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPayRunSchedule? Find(string code)
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

    public void Save(LenientPayRunSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPayRunSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient pay run schedules. Empty fields match all.</summary>
public sealed record LenientPayRunScheduleQuery(string Department, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPayRunSchedule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientPayRunSchedule> Apply(List<LenientPayRunSchedule> items)
    {
        var matched = Collections.Filter(items, (LenientPayRunSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient pay run schedule store.</summary>
public static class LenientPayRunScheduleStore
{
    public static ILenientPayRunScheduleStore NewMemoryLenientPayRunScheduleStore()
    {
        return new MemoryLenientPayRunScheduleStore(
            new Dictionary<string, LenientPayRunSchedule>(),
            new List<string>());
    }

    public static ILenientPayRunScheduleStore NewCachedLenientPayRunScheduleStore(MemoryLenientPayRunScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPayRunScheduleStore(inner, LenientPayRunScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LenientPayRunSchedule> FindLenientPayRunSchedules(ILenientPayRunScheduleStore store, LenientPayRunScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
