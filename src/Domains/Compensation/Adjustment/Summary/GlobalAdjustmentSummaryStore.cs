namespace Acme.Workforce.Compensation.Adjustment.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global adjustment summaries by code.</summary>
public interface IGlobalAdjustmentSummaryStore
{
    GlobalAdjustmentSummary? Find(string code);
    void Save(GlobalAdjustmentSummary item);
    bool Remove(string code);
    List<GlobalAdjustmentSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAdjustmentSummaryStore(Dictionary<string, GlobalAdjustmentSummary> items, List<string> order) : IGlobalAdjustmentSummaryStore
{
    public Dictionary<string, GlobalAdjustmentSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAdjustmentSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAdjustmentSummary item)
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

    public List<GlobalAdjustmentSummary> List()
    {
        var outValue = new List<GlobalAdjustmentSummary>();
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
public sealed class CachedGlobalAdjustmentSummaryStore(MemoryGlobalAdjustmentSummaryStore inner, GlobalAdjustmentSummaryCache cache, int hits, int misses) : IGlobalAdjustmentSummaryStore
{
    public MemoryGlobalAdjustmentSummaryStore Inner { get; set; } = inner;
    public GlobalAdjustmentSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAdjustmentSummary? Find(string code)
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

    public void Save(GlobalAdjustmentSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAdjustmentSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global adjustment summaries. Empty fields match all.</summary>
public sealed record GlobalAdjustmentSummaryQuery(string Region, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAdjustmentSummary item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalAdjustmentSummary> Apply(List<GlobalAdjustmentSummary> items)
    {
        var matched = Collections.Filter(items, (GlobalAdjustmentSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global adjustment summary store.</summary>
public static class GlobalAdjustmentSummaryStore
{
    public static IGlobalAdjustmentSummaryStore NewMemoryGlobalAdjustmentSummaryStore()
    {
        return new MemoryGlobalAdjustmentSummaryStore(
            new Dictionary<string, GlobalAdjustmentSummary>(),
            new List<string>());
    }

    public static IGlobalAdjustmentSummaryStore NewCachedGlobalAdjustmentSummaryStore(MemoryGlobalAdjustmentSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAdjustmentSummaryStore(
            inner,
            GlobalAdjustmentSummaryCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<GlobalAdjustmentSummary> FindGlobalAdjustmentSummaries(IGlobalAdjustmentSummaryStore store, GlobalAdjustmentSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
