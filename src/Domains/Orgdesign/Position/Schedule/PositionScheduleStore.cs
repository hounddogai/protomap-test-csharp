namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position schedules by code.</summary>
public interface IPositionScheduleStore
{
    PositionSchedule? Find(string code);
    void Save(PositionSchedule item);
    bool Remove(string code);
    List<PositionSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionScheduleStore(Dictionary<string, PositionSchedule> items, List<string> order) : IPositionScheduleStore
{
    public Dictionary<string, PositionSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionSchedule item)
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

    public List<PositionSchedule> List()
    {
        var outValue = new List<PositionSchedule>();
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
public sealed class CachedPositionScheduleStore(MemoryPositionScheduleStore inner, PositionScheduleCache cache, int hits, int misses) : IPositionScheduleStore
{
    public MemoryPositionScheduleStore Inner { get; set; } = inner;
    public PositionScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionSchedule? Find(string code)
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

    public void Save(PositionSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position schedules. Empty fields match all.</summary>
public sealed record PositionScheduleQuery(string Department, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PositionSchedule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionSchedule> Apply(List<PositionSchedule> items)
    {
        var matched = Collections.Filter(items, (PositionSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position schedule store.</summary>
public static class PositionScheduleStore
{
    public static IPositionScheduleStore NewMemoryPositionScheduleStore()
    {
        return new MemoryPositionScheduleStore(new Dictionary<string, PositionSchedule>(), new List<string>());
    }

    public static IPositionScheduleStore NewCachedPositionScheduleStore(MemoryPositionScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedPositionScheduleStore(inner, PositionScheduleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PositionSchedule> FindPositionSchedules(IPositionScheduleStore store, PositionScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
