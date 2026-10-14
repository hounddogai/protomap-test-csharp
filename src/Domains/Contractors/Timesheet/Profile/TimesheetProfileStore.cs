namespace Acme.Workforce.Contractors.Timesheet.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet profiles by code.</summary>
public interface ITimesheetProfileStore
{
    TimesheetProfile? Find(string code);
    void Save(TimesheetProfile item);
    bool Remove(string code);
    List<TimesheetProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetProfileStore(Dictionary<string, TimesheetProfile> items, List<string> order) : ITimesheetProfileStore
{
    public Dictionary<string, TimesheetProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetProfile item)
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

    public List<TimesheetProfile> List()
    {
        var outValue = new List<TimesheetProfile>();
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
public sealed class CachedTimesheetProfileStore(MemoryTimesheetProfileStore inner, TimesheetProfileCache cache, int hits, int misses) : ITimesheetProfileStore
{
    public MemoryTimesheetProfileStore Inner { get; set; } = inner;
    public TimesheetProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetProfile? Find(string code)
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

    public void Save(TimesheetProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet profiles. Empty fields match all.</summary>
public sealed record TimesheetProfileQuery(string Team, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetProfile item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetProfile> Apply(List<TimesheetProfile> items)
    {
        var matched = Collections.Filter(items, (TimesheetProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet profile store.</summary>
public static class TimesheetProfileStore
{
    public static ITimesheetProfileStore NewMemoryTimesheetProfileStore()
    {
        return new MemoryTimesheetProfileStore(new Dictionary<string, TimesheetProfile>(), new List<string>());
    }

    public static ITimesheetProfileStore NewCachedTimesheetProfileStore(MemoryTimesheetProfileStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetProfileStore(inner, TimesheetProfileCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TimesheetProfile> FindTimesheetProfiles(ITimesheetProfileStore store, TimesheetProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
