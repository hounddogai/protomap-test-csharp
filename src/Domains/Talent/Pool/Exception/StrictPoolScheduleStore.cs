namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict pool schedules by code.</summary>
public interface IStrictPoolScheduleStore
{
    StrictPoolSchedule? Find(string code);
    void Save(StrictPoolSchedule item);
    bool Remove(string code);
    List<StrictPoolSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPoolScheduleStore(Dictionary<string, StrictPoolSchedule> items, List<string> order) : IStrictPoolScheduleStore
{
    public Dictionary<string, StrictPoolSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPoolSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPoolSchedule item)
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

    public List<StrictPoolSchedule> List()
    {
        var outValue = new List<StrictPoolSchedule>();
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
public sealed class CachedStrictPoolScheduleStore(MemoryStrictPoolScheduleStore inner, StrictPoolScheduleCache cache, int hits, int misses) : IStrictPoolScheduleStore
{
    public MemoryStrictPoolScheduleStore Inner { get; set; } = inner;
    public StrictPoolScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPoolSchedule? Find(string code)
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

    public void Save(StrictPoolSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPoolSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict pool schedules. Empty fields match all.</summary>
public sealed record StrictPoolScheduleQuery(string Source, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPoolSchedule item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<StrictPoolSchedule> Apply(List<StrictPoolSchedule> items)
    {
        var matched = Collections.Filter(items, (StrictPoolSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict pool schedule store.</summary>
public static class StrictPoolScheduleStore
{
    public static IStrictPoolScheduleStore NewMemoryStrictPoolScheduleStore()
    {
        return new MemoryStrictPoolScheduleStore(new Dictionary<string, StrictPoolSchedule>(), new List<string>());
    }

    public static IStrictPoolScheduleStore NewCachedStrictPoolScheduleStore(MemoryStrictPoolScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPoolScheduleStore(inner, StrictPoolScheduleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StrictPoolSchedule> FindStrictPoolSchedules(IStrictPoolScheduleStore store, StrictPoolScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
