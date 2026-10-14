namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient timesheet preferences by code.</summary>
public interface ILenientTimesheetPreferenceStore
{
    LenientTimesheetPreference? Find(string code);
    void Save(LenientTimesheetPreference item);
    bool Remove(string code);
    List<LenientTimesheetPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientTimesheetPreferenceStore(Dictionary<string, LenientTimesheetPreference> items, List<string> order) : ILenientTimesheetPreferenceStore
{
    public Dictionary<string, LenientTimesheetPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientTimesheetPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientTimesheetPreference item)
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

    public List<LenientTimesheetPreference> List()
    {
        var outValue = new List<LenientTimesheetPreference>();
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
public sealed class CachedLenientTimesheetPreferenceStore(MemoryLenientTimesheetPreferenceStore inner, LenientTimesheetPreferenceCache cache, int hits, int misses) : ILenientTimesheetPreferenceStore
{
    public MemoryLenientTimesheetPreferenceStore Inner { get; set; } = inner;
    public LenientTimesheetPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientTimesheetPreference? Find(string code)
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

    public void Save(LenientTimesheetPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientTimesheetPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient timesheet preferences. Empty fields match all.</summary>
public sealed record LenientTimesheetPreferenceQuery(string Country, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(LenientTimesheetPreference item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientTimesheetPreference> Apply(List<LenientTimesheetPreference> items)
    {
        var matched = Collections.Filter(items, (LenientTimesheetPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient timesheet preference store.</summary>
public static class LenientTimesheetPreferenceStore
{
    public static ILenientTimesheetPreferenceStore NewMemoryLenientTimesheetPreferenceStore()
    {
        return new MemoryLenientTimesheetPreferenceStore(
            new Dictionary<string, LenientTimesheetPreference>(),
            new List<string>());
    }

    public static ILenientTimesheetPreferenceStore NewCachedLenientTimesheetPreferenceStore(MemoryLenientTimesheetPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedLenientTimesheetPreferenceStore(
            inner,
            LenientTimesheetPreferenceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientTimesheetPreference> FindLenientTimesheetPreferences(ILenientTimesheetPreferenceStore store, LenientTimesheetPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
