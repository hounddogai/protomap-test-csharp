namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled reorg schedules by code.</summary>
public interface IScheduledReorgScheduleStore
{
    ScheduledReorgSchedule? Find(string code);
    void Save(ScheduledReorgSchedule item);
    bool Remove(string code);
    List<ScheduledReorgSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledReorgScheduleStore(Dictionary<string, ScheduledReorgSchedule> items, List<string> order) : IScheduledReorgScheduleStore
{
    public Dictionary<string, ScheduledReorgSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledReorgSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledReorgSchedule item)
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

    public List<ScheduledReorgSchedule> List()
    {
        var outValue = new List<ScheduledReorgSchedule>();
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
public sealed class CachedScheduledReorgScheduleStore(MemoryScheduledReorgScheduleStore inner, ScheduledReorgScheduleCache cache, int hits, int misses) : IScheduledReorgScheduleStore
{
    public MemoryScheduledReorgScheduleStore Inner { get; set; } = inner;
    public ScheduledReorgScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledReorgSchedule? Find(string code)
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

    public void Save(ScheduledReorgSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledReorgSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled reorg schedules. Empty fields match all.</summary>
public sealed record ScheduledReorgScheduleQuery(string Unit, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledReorgSchedule item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledReorgSchedule> Apply(List<ScheduledReorgSchedule> items)
    {
        var matched = Collections.Filter(items, (ScheduledReorgSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled reorg schedule store.</summary>
public static class ScheduledReorgScheduleStore
{
    public static IScheduledReorgScheduleStore NewMemoryScheduledReorgScheduleStore()
    {
        return new MemoryScheduledReorgScheduleStore(
            new Dictionary<string, ScheduledReorgSchedule>(),
            new List<string>());
    }

    public static IScheduledReorgScheduleStore NewCachedScheduledReorgScheduleStore(MemoryScheduledReorgScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledReorgScheduleStore(
            inner,
            ScheduledReorgScheduleCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledReorgSchedule> FindScheduledReorgSchedules(IScheduledReorgScheduleStore store, ScheduledReorgScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
