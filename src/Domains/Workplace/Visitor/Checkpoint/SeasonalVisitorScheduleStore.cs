namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal visitor schedules by code.</summary>
public interface ISeasonalVisitorScheduleStore
{
    SeasonalVisitorSchedule? Find(string code);
    void Save(SeasonalVisitorSchedule item);
    bool Remove(string code);
    List<SeasonalVisitorSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalVisitorScheduleStore(Dictionary<string, SeasonalVisitorSchedule> items, List<string> order) : ISeasonalVisitorScheduleStore
{
    public Dictionary<string, SeasonalVisitorSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalVisitorSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalVisitorSchedule item)
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

    public List<SeasonalVisitorSchedule> List()
    {
        var outValue = new List<SeasonalVisitorSchedule>();
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
public sealed class CachedSeasonalVisitorScheduleStore(MemorySeasonalVisitorScheduleStore inner, SeasonalVisitorScheduleCache cache, int hits, int misses) : ISeasonalVisitorScheduleStore
{
    public MemorySeasonalVisitorScheduleStore Inner { get; set; } = inner;
    public SeasonalVisitorScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalVisitorSchedule? Find(string code)
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

    public void Save(SeasonalVisitorSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalVisitorSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal visitor schedules. Empty fields match all.</summary>
public sealed record SeasonalVisitorScheduleQuery(string Department, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalVisitorSchedule item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalVisitorSchedule> Apply(List<SeasonalVisitorSchedule> items)
    {
        var matched = Collections.Filter(items, (SeasonalVisitorSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal visitor schedule store.</summary>
public static class SeasonalVisitorScheduleStore
{
    public static ISeasonalVisitorScheduleStore NewMemorySeasonalVisitorScheduleStore()
    {
        return new MemorySeasonalVisitorScheduleStore(
            new Dictionary<string, SeasonalVisitorSchedule>(),
            new List<string>());
    }

    public static ISeasonalVisitorScheduleStore NewCachedSeasonalVisitorScheduleStore(MemorySeasonalVisitorScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalVisitorScheduleStore(
            inner,
            SeasonalVisitorScheduleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalVisitorSchedule> FindSeasonalVisitorSchedules(ISeasonalVisitorScheduleStore store, SeasonalVisitorScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
