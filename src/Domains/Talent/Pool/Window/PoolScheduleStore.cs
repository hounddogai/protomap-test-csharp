namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool schedules by code.</summary>
public interface IPoolScheduleStore
{
    PoolSchedule? Find(string code);
    void Save(PoolSchedule item);
    bool Remove(string code);
    List<PoolSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolScheduleStore(Dictionary<string, PoolSchedule> items, List<string> order) : IPoolScheduleStore
{
    public Dictionary<string, PoolSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolSchedule item)
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

    public List<PoolSchedule> List()
    {
        var outValue = new List<PoolSchedule>();
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
public sealed class CachedPoolScheduleStore(MemoryPoolScheduleStore inner, PoolScheduleCache cache, int hits, int misses) : IPoolScheduleStore
{
    public MemoryPoolScheduleStore Inner { get; set; } = inner;
    public PoolScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolSchedule? Find(string code)
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

    public void Save(PoolSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool schedules. Empty fields match all.</summary>
public sealed record PoolScheduleQuery(string Department, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PoolSchedule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolSchedule> Apply(List<PoolSchedule> items)
    {
        var matched = Collections.Filter(items, (PoolSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool schedule store.</summary>
public static class PoolScheduleStore
{
    public static IPoolScheduleStore NewMemoryPoolScheduleStore()
    {
        return new MemoryPoolScheduleStore(new Dictionary<string, PoolSchedule>(), new List<string>());
    }

    public static IPoolScheduleStore NewCachedPoolScheduleStore(MemoryPoolScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedPoolScheduleStore(inner, PoolScheduleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PoolSchedule> FindPoolSchedules(IPoolScheduleStore store, PoolScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
