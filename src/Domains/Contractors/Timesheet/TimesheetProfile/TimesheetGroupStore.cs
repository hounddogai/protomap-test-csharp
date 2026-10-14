namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet groups by code.</summary>
public interface ITimesheetGroupStore
{
    TimesheetGroup? Find(string code);
    void Save(TimesheetGroup item);
    bool Remove(string code);
    List<TimesheetGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetGroupStore(Dictionary<string, TimesheetGroup> items, List<string> order) : ITimesheetGroupStore
{
    public Dictionary<string, TimesheetGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetGroup item)
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

    public List<TimesheetGroup> List()
    {
        var outValue = new List<TimesheetGroup>();
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
public sealed class CachedTimesheetGroupStore(MemoryTimesheetGroupStore inner, TimesheetGroupCache cache, int hits, int misses) : ITimesheetGroupStore
{
    public MemoryTimesheetGroupStore Inner { get; set; } = inner;
    public TimesheetGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetGroup? Find(string code)
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

    public void Save(TimesheetGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet groups. Empty fields match all.</summary>
public sealed record TimesheetGroupQuery(string Title, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetGroup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetGroup> Apply(List<TimesheetGroup> items)
    {
        var matched = Collections.Filter(items, (TimesheetGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet group store.</summary>
public static class TimesheetGroupStore
{
    public static ITimesheetGroupStore NewMemoryTimesheetGroupStore()
    {
        return new MemoryTimesheetGroupStore(new Dictionary<string, TimesheetGroup>(), new List<string>());
    }

    public static ITimesheetGroupStore NewCachedTimesheetGroupStore(MemoryTimesheetGroupStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetGroupStore(inner, TimesheetGroupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TimesheetGroup> FindTimesheetGroups(ITimesheetGroupStore store, TimesheetGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
