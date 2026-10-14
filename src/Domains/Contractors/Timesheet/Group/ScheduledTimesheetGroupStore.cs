namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled timesheet groups by code.</summary>
public interface IScheduledTimesheetGroupStore
{
    ScheduledTimesheetGroup? Find(string code);
    void Save(ScheduledTimesheetGroup item);
    bool Remove(string code);
    List<ScheduledTimesheetGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledTimesheetGroupStore(Dictionary<string, ScheduledTimesheetGroup> items, List<string> order) : IScheduledTimesheetGroupStore
{
    public Dictionary<string, ScheduledTimesheetGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledTimesheetGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledTimesheetGroup item)
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

    public List<ScheduledTimesheetGroup> List()
    {
        var outValue = new List<ScheduledTimesheetGroup>();
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
public sealed class CachedScheduledTimesheetGroupStore(MemoryScheduledTimesheetGroupStore inner, ScheduledTimesheetGroupCache cache, int hits, int misses) : IScheduledTimesheetGroupStore
{
    public MemoryScheduledTimesheetGroupStore Inner { get; set; } = inner;
    public ScheduledTimesheetGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledTimesheetGroup? Find(string code)
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

    public void Save(ScheduledTimesheetGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledTimesheetGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled timesheet groups. Empty fields match all.</summary>
public sealed record ScheduledTimesheetGroupQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledTimesheetGroup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<ScheduledTimesheetGroup> Apply(List<ScheduledTimesheetGroup> items)
    {
        var matched = Collections.Filter(items, (ScheduledTimesheetGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled timesheet group store.</summary>
public static class ScheduledTimesheetGroupStore
{
    public static IScheduledTimesheetGroupStore NewMemoryScheduledTimesheetGroupStore()
    {
        return new MemoryScheduledTimesheetGroupStore(
            new Dictionary<string, ScheduledTimesheetGroup>(),
            new List<string>());
    }

    public static IScheduledTimesheetGroupStore NewCachedScheduledTimesheetGroupStore(MemoryScheduledTimesheetGroupStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledTimesheetGroupStore(
            inner,
            ScheduledTimesheetGroupCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledTimesheetGroup> FindScheduledTimesheetGroups(IScheduledTimesheetGroupStore store, ScheduledTimesheetGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
