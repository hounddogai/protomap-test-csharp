namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped feedback summaries by code.</summary>
public interface ICappedFeedbackSummaryStore
{
    CappedFeedbackSummary? Find(string code);
    void Save(CappedFeedbackSummary item);
    bool Remove(string code);
    List<CappedFeedbackSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedFeedbackSummaryStore(Dictionary<string, CappedFeedbackSummary> items, List<string> order) : ICappedFeedbackSummaryStore
{
    public Dictionary<string, CappedFeedbackSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedFeedbackSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedFeedbackSummary item)
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

    public List<CappedFeedbackSummary> List()
    {
        var outValue = new List<CappedFeedbackSummary>();
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
public sealed class CachedCappedFeedbackSummaryStore(MemoryCappedFeedbackSummaryStore inner, CappedFeedbackSummaryCache cache, int hits, int misses) : ICappedFeedbackSummaryStore
{
    public MemoryCappedFeedbackSummaryStore Inner { get; set; } = inner;
    public CappedFeedbackSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedFeedbackSummary? Find(string code)
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

    public void Save(CappedFeedbackSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedFeedbackSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped feedback summaries. Empty fields match all.</summary>
public sealed record CappedFeedbackSummaryQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(CappedFeedbackSummary item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedFeedbackSummary> Apply(List<CappedFeedbackSummary> items)
    {
        var matched = Collections.Filter(items, (CappedFeedbackSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped feedback summary store.</summary>
public static class CappedFeedbackSummaryStore
{
    public static ICappedFeedbackSummaryStore NewMemoryCappedFeedbackSummaryStore()
    {
        return new MemoryCappedFeedbackSummaryStore(
            new Dictionary<string, CappedFeedbackSummary>(),
            new List<string>());
    }

    public static ICappedFeedbackSummaryStore NewCachedCappedFeedbackSummaryStore(MemoryCappedFeedbackSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedCappedFeedbackSummaryStore(inner, CappedFeedbackSummaryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CappedFeedbackSummary> FindCappedFeedbackSummaries(ICappedFeedbackSummaryStore store, CappedFeedbackSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
