namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending timesheet summaries by code.</summary>
public interface IPendingTimesheetSummaryStore
{
    PendingTimesheetSummary? Find(string code);
    void Save(PendingTimesheetSummary item);
    bool Remove(string code);
    List<PendingTimesheetSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingTimesheetSummaryStore(Dictionary<string, PendingTimesheetSummary> items, List<string> order) : IPendingTimesheetSummaryStore
{
    public Dictionary<string, PendingTimesheetSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingTimesheetSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingTimesheetSummary item)
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

    public List<PendingTimesheetSummary> List()
    {
        var outValue = new List<PendingTimesheetSummary>();
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
public sealed class CachedPendingTimesheetSummaryStore(MemoryPendingTimesheetSummaryStore inner, PendingTimesheetSummaryCache cache, int hits, int misses) : IPendingTimesheetSummaryStore
{
    public MemoryPendingTimesheetSummaryStore Inner { get; set; } = inner;
    public PendingTimesheetSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingTimesheetSummary? Find(string code)
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

    public void Save(PendingTimesheetSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingTimesheetSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending timesheet summaries. Empty fields match all.</summary>
public sealed record PendingTimesheetSummaryQuery(string Category, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PendingTimesheetSummary item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingTimesheetSummary> Apply(List<PendingTimesheetSummary> items)
    {
        var matched = Collections.Filter(items, (PendingTimesheetSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending timesheet summary store.</summary>
public static class PendingTimesheetSummaryStore
{
    public static IPendingTimesheetSummaryStore NewMemoryPendingTimesheetSummaryStore()
    {
        return new MemoryPendingTimesheetSummaryStore(
            new Dictionary<string, PendingTimesheetSummary>(),
            new List<string>());
    }

    public static IPendingTimesheetSummaryStore NewCachedPendingTimesheetSummaryStore(MemoryPendingTimesheetSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedPendingTimesheetSummaryStore(
            inner,
            PendingTimesheetSummaryCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PendingTimesheetSummary> FindPendingTimesheetSummaries(IPendingTimesheetSummaryStore store, PendingTimesheetSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
