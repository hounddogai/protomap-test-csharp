namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet exceptions by code.</summary>
public interface ITimesheetExceptionStore
{
    TimesheetException? Find(string code);
    void Save(TimesheetException item);
    bool Remove(string code);
    List<TimesheetException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetExceptionStore(Dictionary<string, TimesheetException> items, List<string> order) : ITimesheetExceptionStore
{
    public Dictionary<string, TimesheetException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetException item)
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

    public List<TimesheetException> List()
    {
        var outValue = new List<TimesheetException>();
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
public sealed class CachedTimesheetExceptionStore(MemoryTimesheetExceptionStore inner, TimesheetExceptionCache cache, int hits, int misses) : ITimesheetExceptionStore
{
    public MemoryTimesheetExceptionStore Inner { get; set; } = inner;
    public TimesheetExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetException? Find(string code)
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

    public void Save(TimesheetException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet exceptions. Empty fields match all.</summary>
public sealed record TimesheetExceptionQuery(string Team, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetException item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetException> Apply(List<TimesheetException> items)
    {
        var matched = Collections.Filter(items, (TimesheetException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet exception store.</summary>
public static class TimesheetExceptionStore
{
    public static ITimesheetExceptionStore NewMemoryTimesheetExceptionStore()
    {
        return new MemoryTimesheetExceptionStore(new Dictionary<string, TimesheetException>(), new List<string>());
    }

    public static ITimesheetExceptionStore NewCachedTimesheetExceptionStore(MemoryTimesheetExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetExceptionStore(inner, TimesheetExceptionCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TimesheetException> FindTimesheetExceptions(ITimesheetExceptionStore store, TimesheetExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
