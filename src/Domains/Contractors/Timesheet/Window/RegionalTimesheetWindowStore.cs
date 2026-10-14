namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional timesheet windows by code.</summary>
public interface IRegionalTimesheetWindowStore
{
    RegionalTimesheetWindow? Find(string code);
    void Save(RegionalTimesheetWindow item);
    bool Remove(string code);
    List<RegionalTimesheetWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalTimesheetWindowStore(Dictionary<string, RegionalTimesheetWindow> items, List<string> order) : IRegionalTimesheetWindowStore
{
    public Dictionary<string, RegionalTimesheetWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalTimesheetWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalTimesheetWindow item)
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

    public List<RegionalTimesheetWindow> List()
    {
        var outValue = new List<RegionalTimesheetWindow>();
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
public sealed class CachedRegionalTimesheetWindowStore(MemoryRegionalTimesheetWindowStore inner, RegionalTimesheetWindowCache cache, int hits, int misses) : IRegionalTimesheetWindowStore
{
    public MemoryRegionalTimesheetWindowStore Inner { get; set; } = inner;
    public RegionalTimesheetWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalTimesheetWindow? Find(string code)
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

    public void Save(RegionalTimesheetWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalTimesheetWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional timesheet windows. Empty fields match all.</summary>
public sealed record RegionalTimesheetWindowQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalTimesheetWindow item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalTimesheetWindow> Apply(List<RegionalTimesheetWindow> items)
    {
        var matched = Collections.Filter(items, (RegionalTimesheetWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional timesheet window store.</summary>
public static class RegionalTimesheetWindowStore
{
    public static IRegionalTimesheetWindowStore NewMemoryRegionalTimesheetWindowStore()
    {
        return new MemoryRegionalTimesheetWindowStore(
            new Dictionary<string, RegionalTimesheetWindow>(),
            new List<string>());
    }

    public static IRegionalTimesheetWindowStore NewCachedRegionalTimesheetWindowStore(MemoryRegionalTimesheetWindowStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalTimesheetWindowStore(
            inner,
            RegionalTimesheetWindowCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RegionalTimesheetWindow> FindRegionalTimesheetWindows(IRegionalTimesheetWindowStore store, RegionalTimesheetWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
