namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient training due schedules by code.</summary>
public interface ILenientTrainingDueScheduleStore
{
    LenientTrainingDueSchedule? Find(string code);
    void Save(LenientTrainingDueSchedule item);
    bool Remove(string code);
    List<LenientTrainingDueSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientTrainingDueScheduleStore(Dictionary<string, LenientTrainingDueSchedule> items, List<string> order) : ILenientTrainingDueScheduleStore
{
    public Dictionary<string, LenientTrainingDueSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientTrainingDueSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientTrainingDueSchedule item)
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

    public List<LenientTrainingDueSchedule> List()
    {
        var outValue = new List<LenientTrainingDueSchedule>();
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
public sealed class CachedLenientTrainingDueScheduleStore(MemoryLenientTrainingDueScheduleStore inner, LenientTrainingDueScheduleCache cache, int hits, int misses) : ILenientTrainingDueScheduleStore
{
    public MemoryLenientTrainingDueScheduleStore Inner { get; set; } = inner;
    public LenientTrainingDueScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientTrainingDueSchedule? Find(string code)
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

    public void Save(LenientTrainingDueSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientTrainingDueSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient training due schedules. Empty fields match all.</summary>
public sealed record LenientTrainingDueScheduleQuery(string Segment, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(LenientTrainingDueSchedule item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<LenientTrainingDueSchedule> Apply(List<LenientTrainingDueSchedule> items)
    {
        var matched = Collections.Filter(items, (LenientTrainingDueSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient training due schedule store.</summary>
public static class LenientTrainingDueScheduleStore
{
    public static ILenientTrainingDueScheduleStore NewMemoryLenientTrainingDueScheduleStore()
    {
        return new MemoryLenientTrainingDueScheduleStore(
            new Dictionary<string, LenientTrainingDueSchedule>(),
            new List<string>());
    }

    public static ILenientTrainingDueScheduleStore NewCachedLenientTrainingDueScheduleStore(MemoryLenientTrainingDueScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedLenientTrainingDueScheduleStore(
            inner,
            LenientTrainingDueScheduleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LenientTrainingDueSchedule> FindLenientTrainingDueSchedules(ILenientTrainingDueScheduleStore store, LenientTrainingDueScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
