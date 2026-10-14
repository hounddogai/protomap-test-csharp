namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark schedules by code.</summary>
public interface IBenchmarkScheduleStore
{
    BenchmarkSchedule? Find(string code);
    void Save(BenchmarkSchedule item);
    bool Remove(string code);
    List<BenchmarkSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkScheduleStore(Dictionary<string, BenchmarkSchedule> items, List<string> order) : IBenchmarkScheduleStore
{
    public Dictionary<string, BenchmarkSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkSchedule item)
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

    public List<BenchmarkSchedule> List()
    {
        var outValue = new List<BenchmarkSchedule>();
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
public sealed class CachedBenchmarkScheduleStore(MemoryBenchmarkScheduleStore inner, BenchmarkScheduleCache cache, int hits, int misses) : IBenchmarkScheduleStore
{
    public MemoryBenchmarkScheduleStore Inner { get; set; } = inner;
    public BenchmarkScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkSchedule? Find(string code)
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

    public void Save(BenchmarkSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark schedules. Empty fields match all.</summary>
public sealed record BenchmarkScheduleQuery(string Channel, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkSchedule item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkSchedule> Apply(List<BenchmarkSchedule> items)
    {
        var matched = Collections.Filter(items, (BenchmarkSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark schedule store.</summary>
public static class BenchmarkScheduleStore
{
    public static IBenchmarkScheduleStore NewMemoryBenchmarkScheduleStore()
    {
        return new MemoryBenchmarkScheduleStore(new Dictionary<string, BenchmarkSchedule>(), new List<string>());
    }

    public static IBenchmarkScheduleStore NewCachedBenchmarkScheduleStore(MemoryBenchmarkScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkScheduleStore(inner, BenchmarkScheduleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BenchmarkSchedule> FindBenchmarkSchedules(IBenchmarkScheduleStore store, BenchmarkScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
