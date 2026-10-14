namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet bundles by code.</summary>
public interface ITimesheetBundleStore
{
    TimesheetBundle? Find(string code);
    void Save(TimesheetBundle item);
    bool Remove(string code);
    List<TimesheetBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetBundleStore(Dictionary<string, TimesheetBundle> items, List<string> order) : ITimesheetBundleStore
{
    public Dictionary<string, TimesheetBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetBundle item)
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

    public List<TimesheetBundle> List()
    {
        var outValue = new List<TimesheetBundle>();
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
public sealed class CachedTimesheetBundleStore(MemoryTimesheetBundleStore inner, TimesheetBundleCache cache, int hits, int misses) : ITimesheetBundleStore
{
    public MemoryTimesheetBundleStore Inner { get; set; } = inner;
    public TimesheetBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetBundle? Find(string code)
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

    public void Save(TimesheetBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet bundles. Empty fields match all.</summary>
public sealed record TimesheetBundleQuery(string Category, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetBundle item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<TimesheetBundle> Apply(List<TimesheetBundle> items)
    {
        var matched = Collections.Filter(items, (TimesheetBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet bundle store.</summary>
public static class TimesheetBundleStore
{
    public static ITimesheetBundleStore NewMemoryTimesheetBundleStore()
    {
        return new MemoryTimesheetBundleStore(new Dictionary<string, TimesheetBundle>(), new List<string>());
    }

    public static ITimesheetBundleStore NewCachedTimesheetBundleStore(MemoryTimesheetBundleStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetBundleStore(inner, TimesheetBundleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TimesheetBundle> FindTimesheetBundles(ITimesheetBundleStore store, TimesheetBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
