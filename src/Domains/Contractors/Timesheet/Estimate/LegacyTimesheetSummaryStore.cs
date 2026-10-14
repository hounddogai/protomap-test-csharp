namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy timesheet summaries by code.</summary>
public interface ILegacyTimesheetSummaryStore
{
    LegacyTimesheetSummary? Find(string code);
    void Save(LegacyTimesheetSummary item);
    bool Remove(string code);
    List<LegacyTimesheetSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyTimesheetSummaryStore(Dictionary<string, LegacyTimesheetSummary> items, List<string> order) : ILegacyTimesheetSummaryStore
{
    public Dictionary<string, LegacyTimesheetSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyTimesheetSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyTimesheetSummary item)
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

    public List<LegacyTimesheetSummary> List()
    {
        var outValue = new List<LegacyTimesheetSummary>();
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
public sealed class CachedLegacyTimesheetSummaryStore(MemoryLegacyTimesheetSummaryStore inner, LegacyTimesheetSummaryCache cache, int hits, int misses) : ILegacyTimesheetSummaryStore
{
    public MemoryLegacyTimesheetSummaryStore Inner { get; set; } = inner;
    public LegacyTimesheetSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyTimesheetSummary? Find(string code)
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

    public void Save(LegacyTimesheetSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyTimesheetSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy timesheet summaries. Empty fields match all.</summary>
public sealed record LegacyTimesheetSummaryQuery(string Country, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyTimesheetSummary item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyTimesheetSummary> Apply(List<LegacyTimesheetSummary> items)
    {
        var matched = Collections.Filter(items, (LegacyTimesheetSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy timesheet summary store.</summary>
public static class LegacyTimesheetSummaryStore
{
    public static ILegacyTimesheetSummaryStore NewMemoryLegacyTimesheetSummaryStore()
    {
        return new MemoryLegacyTimesheetSummaryStore(
            new Dictionary<string, LegacyTimesheetSummary>(),
            new List<string>());
    }

    public static ILegacyTimesheetSummaryStore NewCachedLegacyTimesheetSummaryStore(MemoryLegacyTimesheetSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyTimesheetSummaryStore(
            inner,
            LegacyTimesheetSummaryCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyTimesheetSummary> FindLegacyTimesheetSummaries(ILegacyTimesheetSummaryStore store, LegacyTimesheetSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
