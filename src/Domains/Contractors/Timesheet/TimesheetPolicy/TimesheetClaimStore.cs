namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet claims by code.</summary>
public interface ITimesheetClaimStore
{
    TimesheetClaim? Find(string code);
    void Save(TimesheetClaim item);
    bool Remove(string code);
    List<TimesheetClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetClaimStore(Dictionary<string, TimesheetClaim> items, List<string> order) : ITimesheetClaimStore
{
    public Dictionary<string, TimesheetClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetClaim item)
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

    public List<TimesheetClaim> List()
    {
        var outValue = new List<TimesheetClaim>();
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
public sealed class CachedTimesheetClaimStore(MemoryTimesheetClaimStore inner, TimesheetClaimCache cache, int hits, int misses) : ITimesheetClaimStore
{
    public MemoryTimesheetClaimStore Inner { get; set; } = inner;
    public TimesheetClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetClaim? Find(string code)
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

    public void Save(TimesheetClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet claims. Empty fields match all.</summary>
public sealed record TimesheetClaimQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetClaim item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetClaim> Apply(List<TimesheetClaim> items)
    {
        var matched = Collections.Filter(items, (TimesheetClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet claim store.</summary>
public static class TimesheetClaimStore
{
    public static ITimesheetClaimStore NewMemoryTimesheetClaimStore()
    {
        return new MemoryTimesheetClaimStore(new Dictionary<string, TimesheetClaim>(), new List<string>());
    }

    public static ITimesheetClaimStore NewCachedTimesheetClaimStore(MemoryTimesheetClaimStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetClaimStore(inner, TimesheetClaimCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TimesheetClaim> FindTimesheetClaims(ITimesheetClaimStore store, TimesheetClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
