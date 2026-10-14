namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment summaries by code.</summary>
public interface IAdjustmentSummaryStore
{
    AdjustmentSummary? Find(string code);
    void Save(AdjustmentSummary item);
    bool Remove(string code);
    List<AdjustmentSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentSummaryStore(Dictionary<string, AdjustmentSummary> items, List<string> order) : IAdjustmentSummaryStore
{
    public Dictionary<string, AdjustmentSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentSummary item)
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

    public List<AdjustmentSummary> List()
    {
        var outValue = new List<AdjustmentSummary>();
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
public sealed class CachedAdjustmentSummaryStore(MemoryAdjustmentSummaryStore inner, AdjustmentSummaryCache cache, int hits, int misses) : IAdjustmentSummaryStore
{
    public MemoryAdjustmentSummaryStore Inner { get; set; } = inner;
    public AdjustmentSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentSummary? Find(string code)
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

    public void Save(AdjustmentSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment summaries. Empty fields match all.</summary>
public sealed record AdjustmentSummaryQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentSummary item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentSummary> Apply(List<AdjustmentSummary> items)
    {
        var matched = Collections.Filter(items, (AdjustmentSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment summary store.</summary>
public static class AdjustmentSummaryStore
{
    public static IAdjustmentSummaryStore NewMemoryAdjustmentSummaryStore()
    {
        return new MemoryAdjustmentSummaryStore(new Dictionary<string, AdjustmentSummary>(), new List<string>());
    }

    public static IAdjustmentSummaryStore NewCachedAdjustmentSummaryStore(MemoryAdjustmentSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentSummaryStore(inner, AdjustmentSummaryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdjustmentSummary> FindAdjustmentSummaries(IAdjustmentSummaryStore store, AdjustmentSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
