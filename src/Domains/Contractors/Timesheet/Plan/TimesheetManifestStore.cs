namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet manifests by code.</summary>
public interface ITimesheetManifestStore
{
    TimesheetManifest? Find(string code);
    void Save(TimesheetManifest item);
    bool Remove(string code);
    List<TimesheetManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetManifestStore(Dictionary<string, TimesheetManifest> items, List<string> order) : ITimesheetManifestStore
{
    public Dictionary<string, TimesheetManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetManifest item)
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

    public List<TimesheetManifest> List()
    {
        var outValue = new List<TimesheetManifest>();
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
public sealed class CachedTimesheetManifestStore(MemoryTimesheetManifestStore inner, TimesheetManifestCache cache, int hits, int misses) : ITimesheetManifestStore
{
    public MemoryTimesheetManifestStore Inner { get; set; } = inner;
    public TimesheetManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetManifest? Find(string code)
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

    public void Save(TimesheetManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet manifests. Empty fields match all.</summary>
public sealed record TimesheetManifestQuery(string Category, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetManifest item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetManifest> Apply(List<TimesheetManifest> items)
    {
        var matched = Collections.Filter(items, (TimesheetManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet manifest store.</summary>
public static class TimesheetManifestStore
{
    public static ITimesheetManifestStore NewMemoryTimesheetManifestStore()
    {
        return new MemoryTimesheetManifestStore(new Dictionary<string, TimesheetManifest>(), new List<string>());
    }

    public static ITimesheetManifestStore NewCachedTimesheetManifestStore(MemoryTimesheetManifestStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetManifestStore(inner, TimesheetManifestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TimesheetManifest> FindTimesheetManifests(ITimesheetManifestStore store, TimesheetManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
