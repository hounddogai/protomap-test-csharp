namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal adjustment summaries by code.</summary>
public interface ISeasonalAdjustmentSummaryStore
{
    SeasonalAdjustmentSummary? Find(string code);
    void Save(SeasonalAdjustmentSummary item);
    bool Remove(string code);
    List<SeasonalAdjustmentSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAdjustmentSummaryStore(Dictionary<string, SeasonalAdjustmentSummary> items, List<string> order) : ISeasonalAdjustmentSummaryStore
{
    public Dictionary<string, SeasonalAdjustmentSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAdjustmentSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAdjustmentSummary item)
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

    public List<SeasonalAdjustmentSummary> List()
    {
        var outValue = new List<SeasonalAdjustmentSummary>();
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
public sealed class CachedSeasonalAdjustmentSummaryStore(MemorySeasonalAdjustmentSummaryStore inner, SeasonalAdjustmentSummaryCache cache, int hits, int misses) : ISeasonalAdjustmentSummaryStore
{
    public MemorySeasonalAdjustmentSummaryStore Inner { get; set; } = inner;
    public SeasonalAdjustmentSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAdjustmentSummary? Find(string code)
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

    public void Save(SeasonalAdjustmentSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAdjustmentSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal adjustment summaries. Empty fields match all.</summary>
public sealed record SeasonalAdjustmentSummaryQuery(string Category, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAdjustmentSummary item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAdjustmentSummary> Apply(List<SeasonalAdjustmentSummary> items)
    {
        var matched = Collections.Filter(items, (SeasonalAdjustmentSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal adjustment summary store.</summary>
public static class SeasonalAdjustmentSummaryStore
{
    public static ISeasonalAdjustmentSummaryStore NewMemorySeasonalAdjustmentSummaryStore()
    {
        return new MemorySeasonalAdjustmentSummaryStore(
            new Dictionary<string, SeasonalAdjustmentSummary>(),
            new List<string>());
    }

    public static ISeasonalAdjustmentSummaryStore NewCachedSeasonalAdjustmentSummaryStore(MemorySeasonalAdjustmentSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAdjustmentSummaryStore(
            inner,
            SeasonalAdjustmentSummaryCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalAdjustmentSummary> FindSeasonalAdjustmentSummaries(ISeasonalAdjustmentSummaryStore store, SeasonalAdjustmentSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
