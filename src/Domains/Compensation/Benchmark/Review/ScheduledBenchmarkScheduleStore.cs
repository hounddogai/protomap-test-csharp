namespace Acme.Workforce.Compensation.Benchmark.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled benchmark schedules by code.</summary>
public interface IScheduledBenchmarkScheduleStore
{
    ScheduledBenchmarkSchedule? Find(string code);
    void Save(ScheduledBenchmarkSchedule item);
    bool Remove(string code);
    List<ScheduledBenchmarkSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledBenchmarkScheduleStore(Dictionary<string, ScheduledBenchmarkSchedule> items, List<string> order) : IScheduledBenchmarkScheduleStore
{
    public Dictionary<string, ScheduledBenchmarkSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledBenchmarkSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledBenchmarkSchedule item)
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

    public List<ScheduledBenchmarkSchedule> List()
    {
        var outValue = new List<ScheduledBenchmarkSchedule>();
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
public sealed class CachedScheduledBenchmarkScheduleStore(MemoryScheduledBenchmarkScheduleStore inner, ScheduledBenchmarkScheduleCache cache, int hits, int misses) : IScheduledBenchmarkScheduleStore
{
    public MemoryScheduledBenchmarkScheduleStore Inner { get; set; } = inner;
    public ScheduledBenchmarkScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledBenchmarkSchedule? Find(string code)
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

    public void Save(ScheduledBenchmarkSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledBenchmarkSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled benchmark schedules. Empty fields match all.</summary>
public sealed record ScheduledBenchmarkScheduleQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledBenchmarkSchedule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledBenchmarkSchedule> Apply(List<ScheduledBenchmarkSchedule> items)
    {
        var matched = Collections.Filter(items, (ScheduledBenchmarkSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled benchmark schedule store.</summary>
public static class ScheduledBenchmarkScheduleStore
{
    public static IScheduledBenchmarkScheduleStore NewMemoryScheduledBenchmarkScheduleStore()
    {
        return new MemoryScheduledBenchmarkScheduleStore(
            new Dictionary<string, ScheduledBenchmarkSchedule>(),
            new List<string>());
    }

    public static IScheduledBenchmarkScheduleStore NewCachedScheduledBenchmarkScheduleStore(MemoryScheduledBenchmarkScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledBenchmarkScheduleStore(
            inner,
            ScheduledBenchmarkScheduleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledBenchmarkSchedule> FindScheduledBenchmarkSchedules(IScheduledBenchmarkScheduleStore store, ScheduledBenchmarkScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
