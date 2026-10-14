namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet tiers by code.</summary>
public interface ITimesheetTierStore
{
    TimesheetTier? Find(string code);
    void Save(TimesheetTier item);
    bool Remove(string code);
    List<TimesheetTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetTierStore(Dictionary<string, TimesheetTier> items, List<string> order) : ITimesheetTierStore
{
    public Dictionary<string, TimesheetTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetTier item)
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

    public List<TimesheetTier> List()
    {
        var outValue = new List<TimesheetTier>();
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
public sealed class CachedTimesheetTierStore(MemoryTimesheetTierStore inner, TimesheetTierCache cache, int hits, int misses) : ITimesheetTierStore
{
    public MemoryTimesheetTierStore Inner { get; set; } = inner;
    public TimesheetTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetTier? Find(string code)
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

    public void Save(TimesheetTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet tiers. Empty fields match all.</summary>
public sealed record TimesheetTierQuery(string Source, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetTier item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetTier> Apply(List<TimesheetTier> items)
    {
        var matched = Collections.Filter(items, (TimesheetTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet tier store.</summary>
public static class TimesheetTierStore
{
    public static ITimesheetTierStore NewMemoryTimesheetTierStore()
    {
        return new MemoryTimesheetTierStore(new Dictionary<string, TimesheetTier>(), new List<string>());
    }

    public static ITimesheetTierStore NewCachedTimesheetTierStore(MemoryTimesheetTierStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetTierStore(inner, TimesheetTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TimesheetTier> FindTimesheetTiers(ITimesheetTierStore store, TimesheetTierQuery query)
    {
        return query.Apply(store.List());
    }
}
