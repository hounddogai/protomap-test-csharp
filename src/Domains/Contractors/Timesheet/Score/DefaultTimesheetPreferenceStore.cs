namespace Acme.Workforce.Contractors.Timesheet.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default timesheet preferences by code.</summary>
public interface IDefaultTimesheetPreferenceStore
{
    DefaultTimesheetPreference? Find(string code);
    void Save(DefaultTimesheetPreference item);
    bool Remove(string code);
    List<DefaultTimesheetPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultTimesheetPreferenceStore(Dictionary<string, DefaultTimesheetPreference> items, List<string> order) : IDefaultTimesheetPreferenceStore
{
    public Dictionary<string, DefaultTimesheetPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultTimesheetPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultTimesheetPreference item)
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

    public List<DefaultTimesheetPreference> List()
    {
        var outValue = new List<DefaultTimesheetPreference>();
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
public sealed class CachedDefaultTimesheetPreferenceStore(MemoryDefaultTimesheetPreferenceStore inner, DefaultTimesheetPreferenceCache cache, int hits, int misses) : IDefaultTimesheetPreferenceStore
{
    public MemoryDefaultTimesheetPreferenceStore Inner { get; set; } = inner;
    public DefaultTimesheetPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultTimesheetPreference? Find(string code)
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

    public void Save(DefaultTimesheetPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultTimesheetPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default timesheet preferences. Empty fields match all.</summary>
public sealed record DefaultTimesheetPreferenceQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultTimesheetPreference item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<DefaultTimesheetPreference> Apply(List<DefaultTimesheetPreference> items)
    {
        var matched = Collections.Filter(items, (DefaultTimesheetPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default timesheet preference store.</summary>
public static class DefaultTimesheetPreferenceStore
{
    public static IDefaultTimesheetPreferenceStore NewMemoryDefaultTimesheetPreferenceStore()
    {
        return new MemoryDefaultTimesheetPreferenceStore(
            new Dictionary<string, DefaultTimesheetPreference>(),
            new List<string>());
    }

    public static IDefaultTimesheetPreferenceStore NewCachedDefaultTimesheetPreferenceStore(MemoryDefaultTimesheetPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultTimesheetPreferenceStore(
            inner,
            DefaultTimesheetPreferenceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultTimesheetPreference> FindDefaultTimesheetPreferences(IDefaultTimesheetPreferenceStore store, DefaultTimesheetPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
