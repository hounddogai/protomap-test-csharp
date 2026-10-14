namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled adjustment summaries by code.</summary>
public interface IScheduledAdjustmentSummaryStore
{
    ScheduledAdjustmentSummary? Find(string code);
    void Save(ScheduledAdjustmentSummary item);
    bool Remove(string code);
    List<ScheduledAdjustmentSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAdjustmentSummaryStore(Dictionary<string, ScheduledAdjustmentSummary> items, List<string> order) : IScheduledAdjustmentSummaryStore
{
    public Dictionary<string, ScheduledAdjustmentSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAdjustmentSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAdjustmentSummary item)
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

    public List<ScheduledAdjustmentSummary> List()
    {
        var outValue = new List<ScheduledAdjustmentSummary>();
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
public sealed class CachedScheduledAdjustmentSummaryStore(MemoryScheduledAdjustmentSummaryStore inner, ScheduledAdjustmentSummaryCache cache, int hits, int misses) : IScheduledAdjustmentSummaryStore
{
    public MemoryScheduledAdjustmentSummaryStore Inner { get; set; } = inner;
    public ScheduledAdjustmentSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAdjustmentSummary? Find(string code)
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

    public void Save(ScheduledAdjustmentSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAdjustmentSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled adjustment summaries. Empty fields match all.</summary>
public sealed record ScheduledAdjustmentSummaryQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAdjustmentSummary item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAdjustmentSummary> Apply(List<ScheduledAdjustmentSummary> items)
    {
        var matched = Collections.Filter(items, (ScheduledAdjustmentSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled adjustment summary store.</summary>
public static class ScheduledAdjustmentSummaryStore
{
    public static IScheduledAdjustmentSummaryStore NewMemoryScheduledAdjustmentSummaryStore()
    {
        return new MemoryScheduledAdjustmentSummaryStore(
            new Dictionary<string, ScheduledAdjustmentSummary>(),
            new List<string>());
    }

    public static IScheduledAdjustmentSummaryStore NewCachedScheduledAdjustmentSummaryStore(MemoryScheduledAdjustmentSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAdjustmentSummaryStore(
            inner,
            ScheduledAdjustmentSummaryCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAdjustmentSummary> FindScheduledAdjustmentSummaries(IScheduledAdjustmentSummaryStore store, ScheduledAdjustmentSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
