namespace Acme.Workforce.Contractors.Timesheet.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet preferences by code.</summary>
public interface ITimesheetPreferenceStore
{
    TimesheetPreference? Find(string code);
    void Save(TimesheetPreference item);
    bool Remove(string code);
    List<TimesheetPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetPreferenceStore(Dictionary<string, TimesheetPreference> items, List<string> order) : ITimesheetPreferenceStore
{
    public Dictionary<string, TimesheetPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetPreference item)
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

    public List<TimesheetPreference> List()
    {
        var outValue = new List<TimesheetPreference>();
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
public sealed class CachedTimesheetPreferenceStore(MemoryTimesheetPreferenceStore inner, TimesheetPreferenceCache cache, int hits, int misses) : ITimesheetPreferenceStore
{
    public MemoryTimesheetPreferenceStore Inner { get; set; } = inner;
    public TimesheetPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetPreference? Find(string code)
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

    public void Save(TimesheetPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet preferences. Empty fields match all.</summary>
public sealed record TimesheetPreferenceQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetPreference item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetPreference> Apply(List<TimesheetPreference> items)
    {
        var matched = Collections.Filter(items, (TimesheetPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet preference store.</summary>
public static class TimesheetPreferenceStore
{
    public static ITimesheetPreferenceStore NewMemoryTimesheetPreferenceStore()
    {
        return new MemoryTimesheetPreferenceStore(new Dictionary<string, TimesheetPreference>(), new List<string>());
    }

    public static ITimesheetPreferenceStore NewCachedTimesheetPreferenceStore(MemoryTimesheetPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetPreferenceStore(inner, TimesheetPreferenceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TimesheetPreference> FindTimesheetPreferences(ITimesheetPreferenceStore store, TimesheetPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
