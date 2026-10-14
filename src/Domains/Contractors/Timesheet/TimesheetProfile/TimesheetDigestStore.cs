namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet digests by code.</summary>
public interface ITimesheetDigestStore
{
    TimesheetDigest? Find(string code);
    void Save(TimesheetDigest item);
    bool Remove(string code);
    List<TimesheetDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetDigestStore(Dictionary<string, TimesheetDigest> items, List<string> order) : ITimesheetDigestStore
{
    public Dictionary<string, TimesheetDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetDigest item)
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

    public List<TimesheetDigest> List()
    {
        var outValue = new List<TimesheetDigest>();
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
public sealed class CachedTimesheetDigestStore(MemoryTimesheetDigestStore inner, TimesheetDigestCache cache, int hits, int misses) : ITimesheetDigestStore
{
    public MemoryTimesheetDigestStore Inner { get; set; } = inner;
    public TimesheetDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetDigest? Find(string code)
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

    public void Save(TimesheetDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet digests. Empty fields match all.</summary>
public sealed record TimesheetDigestQuery(string Category, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetDigest item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<TimesheetDigest> Apply(List<TimesheetDigest> items)
    {
        var matched = Collections.Filter(items, (TimesheetDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet digest store.</summary>
public static class TimesheetDigestStore
{
    public static ITimesheetDigestStore NewMemoryTimesheetDigestStore()
    {
        return new MemoryTimesheetDigestStore(new Dictionary<string, TimesheetDigest>(), new List<string>());
    }

    public static ITimesheetDigestStore NewCachedTimesheetDigestStore(MemoryTimesheetDigestStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetDigestStore(inner, TimesheetDigestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TimesheetDigest> FindTimesheetDigests(ITimesheetDigestStore store, TimesheetDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
