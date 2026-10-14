namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient adjustment schedules by code.</summary>
public interface ILenientAdjustmentScheduleStore
{
    LenientAdjustmentSchedule? Find(string code);
    void Save(LenientAdjustmentSchedule item);
    bool Remove(string code);
    List<LenientAdjustmentSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientAdjustmentScheduleStore(Dictionary<string, LenientAdjustmentSchedule> items, List<string> order) : ILenientAdjustmentScheduleStore
{
    public Dictionary<string, LenientAdjustmentSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientAdjustmentSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientAdjustmentSchedule item)
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

    public List<LenientAdjustmentSchedule> List()
    {
        var outValue = new List<LenientAdjustmentSchedule>();
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
public sealed class CachedLenientAdjustmentScheduleStore(MemoryLenientAdjustmentScheduleStore inner, LenientAdjustmentScheduleCache cache, int hits, int misses) : ILenientAdjustmentScheduleStore
{
    public MemoryLenientAdjustmentScheduleStore Inner { get; set; } = inner;
    public LenientAdjustmentScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientAdjustmentSchedule? Find(string code)
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

    public void Save(LenientAdjustmentSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientAdjustmentSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient adjustment schedules. Empty fields match all.</summary>
public sealed record LenientAdjustmentScheduleQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LenientAdjustmentSchedule item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientAdjustmentSchedule> Apply(List<LenientAdjustmentSchedule> items)
    {
        var matched = Collections.Filter(items, (LenientAdjustmentSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient adjustment schedule store.</summary>
public static class LenientAdjustmentScheduleStore
{
    public static ILenientAdjustmentScheduleStore NewMemoryLenientAdjustmentScheduleStore()
    {
        return new MemoryLenientAdjustmentScheduleStore(
            new Dictionary<string, LenientAdjustmentSchedule>(),
            new List<string>());
    }

    public static ILenientAdjustmentScheduleStore NewCachedLenientAdjustmentScheduleStore(MemoryLenientAdjustmentScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedLenientAdjustmentScheduleStore(
            inner,
            LenientAdjustmentScheduleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientAdjustmentSchedule> FindLenientAdjustmentSchedules(ILenientAdjustmentScheduleStore store, LenientAdjustmentScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
