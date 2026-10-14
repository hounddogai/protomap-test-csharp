namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient visitor schedules by code.</summary>
public interface ILenientVisitorScheduleStore
{
    LenientVisitorSchedule? Find(string code);
    void Save(LenientVisitorSchedule item);
    bool Remove(string code);
    List<LenientVisitorSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientVisitorScheduleStore(Dictionary<string, LenientVisitorSchedule> items, List<string> order) : ILenientVisitorScheduleStore
{
    public Dictionary<string, LenientVisitorSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientVisitorSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientVisitorSchedule item)
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

    public List<LenientVisitorSchedule> List()
    {
        var outValue = new List<LenientVisitorSchedule>();
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
public sealed class CachedLenientVisitorScheduleStore(MemoryLenientVisitorScheduleStore inner, LenientVisitorScheduleCache cache, int hits, int misses) : ILenientVisitorScheduleStore
{
    public MemoryLenientVisitorScheduleStore Inner { get; set; } = inner;
    public LenientVisitorScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientVisitorSchedule? Find(string code)
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

    public void Save(LenientVisitorSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientVisitorSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient visitor schedules. Empty fields match all.</summary>
public sealed record LenientVisitorScheduleQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(LenientVisitorSchedule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<LenientVisitorSchedule> Apply(List<LenientVisitorSchedule> items)
    {
        var matched = Collections.Filter(items, (LenientVisitorSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient visitor schedule store.</summary>
public static class LenientVisitorScheduleStore
{
    public static ILenientVisitorScheduleStore NewMemoryLenientVisitorScheduleStore()
    {
        return new MemoryLenientVisitorScheduleStore(
            new Dictionary<string, LenientVisitorSchedule>(),
            new List<string>());
    }

    public static ILenientVisitorScheduleStore NewCachedLenientVisitorScheduleStore(MemoryLenientVisitorScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedLenientVisitorScheduleStore(
            inner,
            LenientVisitorScheduleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientVisitorSchedule> FindLenientVisitorSchedules(ILenientVisitorScheduleStore store, LenientVisitorScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
