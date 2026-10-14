namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted adjustment summaries by code.</summary>
public interface IWeightedAdjustmentSummaryStore
{
    WeightedAdjustmentSummary? Find(string code);
    void Save(WeightedAdjustmentSummary item);
    bool Remove(string code);
    List<WeightedAdjustmentSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAdjustmentSummaryStore(Dictionary<string, WeightedAdjustmentSummary> items, List<string> order) : IWeightedAdjustmentSummaryStore
{
    public Dictionary<string, WeightedAdjustmentSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAdjustmentSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAdjustmentSummary item)
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

    public List<WeightedAdjustmentSummary> List()
    {
        var outValue = new List<WeightedAdjustmentSummary>();
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
public sealed class CachedWeightedAdjustmentSummaryStore(MemoryWeightedAdjustmentSummaryStore inner, WeightedAdjustmentSummaryCache cache, int hits, int misses) : IWeightedAdjustmentSummaryStore
{
    public MemoryWeightedAdjustmentSummaryStore Inner { get; set; } = inner;
    public WeightedAdjustmentSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAdjustmentSummary? Find(string code)
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

    public void Save(WeightedAdjustmentSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAdjustmentSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted adjustment summaries. Empty fields match all.</summary>
public sealed record WeightedAdjustmentSummaryQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAdjustmentSummary item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAdjustmentSummary> Apply(List<WeightedAdjustmentSummary> items)
    {
        var matched = Collections.Filter(items, (WeightedAdjustmentSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted adjustment summary store.</summary>
public static class WeightedAdjustmentSummaryStore
{
    public static IWeightedAdjustmentSummaryStore NewMemoryWeightedAdjustmentSummaryStore()
    {
        return new MemoryWeightedAdjustmentSummaryStore(
            new Dictionary<string, WeightedAdjustmentSummary>(),
            new List<string>());
    }

    public static IWeightedAdjustmentSummaryStore NewCachedWeightedAdjustmentSummaryStore(MemoryWeightedAdjustmentSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAdjustmentSummaryStore(
            inner,
            WeightedAdjustmentSummaryCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAdjustmentSummary> FindWeightedAdjustmentSummaries(IWeightedAdjustmentSummaryStore store, WeightedAdjustmentSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
