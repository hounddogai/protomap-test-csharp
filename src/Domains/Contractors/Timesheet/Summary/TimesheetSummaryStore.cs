namespace Acme.Workforce.Contractors.Timesheet.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet summaries by code.</summary>
public interface ITimesheetSummaryStore
{
    TimesheetSummary? Find(string code);
    void Save(TimesheetSummary item);
    bool Remove(string code);
    List<TimesheetSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetSummaryStore(Dictionary<string, TimesheetSummary> items, List<string> order) : ITimesheetSummaryStore
{
    public Dictionary<string, TimesheetSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetSummary item)
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

    public List<TimesheetSummary> List()
    {
        var outValue = new List<TimesheetSummary>();
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
public sealed class CachedTimesheetSummaryStore(MemoryTimesheetSummaryStore inner, TimesheetSummaryCache cache, int hits, int misses) : ITimesheetSummaryStore
{
    public MemoryTimesheetSummaryStore Inner { get; set; } = inner;
    public TimesheetSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetSummary? Find(string code)
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

    public void Save(TimesheetSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet summaries. Empty fields match all.</summary>
public sealed record TimesheetSummaryQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetSummary item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetSummary> Apply(List<TimesheetSummary> items)
    {
        var matched = Collections.Filter(items, (TimesheetSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet summary store.</summary>
public static class TimesheetSummaryStore
{
    public static ITimesheetSummaryStore NewMemoryTimesheetSummaryStore()
    {
        return new MemoryTimesheetSummaryStore(new Dictionary<string, TimesheetSummary>(), new List<string>());
    }

    public static ITimesheetSummaryStore NewCachedTimesheetSummaryStore(MemoryTimesheetSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetSummaryStore(inner, TimesheetSummaryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TimesheetSummary> FindTimesheetSummaries(ITimesheetSummaryStore store, TimesheetSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
