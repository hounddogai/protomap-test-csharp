namespace Acme.Workforce.Contractors.Timesheet.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet scores by code.</summary>
public interface ITimesheetScoreStore
{
    TimesheetScore? Find(string code);
    void Save(TimesheetScore item);
    bool Remove(string code);
    List<TimesheetScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetScoreStore(Dictionary<string, TimesheetScore> items, List<string> order) : ITimesheetScoreStore
{
    public Dictionary<string, TimesheetScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetScore item)
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

    public List<TimesheetScore> List()
    {
        var outValue = new List<TimesheetScore>();
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
public sealed class CachedTimesheetScoreStore(MemoryTimesheetScoreStore inner, TimesheetScoreCache cache, int hits, int misses) : ITimesheetScoreStore
{
    public MemoryTimesheetScoreStore Inner { get; set; } = inner;
    public TimesheetScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetScore? Find(string code)
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

    public void Save(TimesheetScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet scores. Empty fields match all.</summary>
public sealed record TimesheetScoreQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetScore item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<TimesheetScore> Apply(List<TimesheetScore> items)
    {
        var matched = Collections.Filter(items, (TimesheetScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet score store.</summary>
public static class TimesheetScoreStore
{
    public static ITimesheetScoreStore NewMemoryTimesheetScoreStore()
    {
        return new MemoryTimesheetScoreStore(new Dictionary<string, TimesheetScore>(), new List<string>());
    }

    public static ITimesheetScoreStore NewCachedTimesheetScoreStore(MemoryTimesheetScoreStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetScoreStore(inner, TimesheetScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TimesheetScore> FindTimesheetScores(ITimesheetScoreStore store, TimesheetScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
