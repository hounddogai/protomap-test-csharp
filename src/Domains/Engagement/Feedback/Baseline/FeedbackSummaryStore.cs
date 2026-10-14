namespace Acme.Workforce.Engagement.Feedback.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback summaries by code.</summary>
public interface IFeedbackSummaryStore
{
    FeedbackSummary? Find(string code);
    void Save(FeedbackSummary item);
    bool Remove(string code);
    List<FeedbackSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackSummaryStore(Dictionary<string, FeedbackSummary> items, List<string> order) : IFeedbackSummaryStore
{
    public Dictionary<string, FeedbackSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackSummary item)
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

    public List<FeedbackSummary> List()
    {
        var outValue = new List<FeedbackSummary>();
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
public sealed class CachedFeedbackSummaryStore(MemoryFeedbackSummaryStore inner, FeedbackSummaryCache cache, int hits, int misses) : IFeedbackSummaryStore
{
    public MemoryFeedbackSummaryStore Inner { get; set; } = inner;
    public FeedbackSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackSummary? Find(string code)
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

    public void Save(FeedbackSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback summaries. Empty fields match all.</summary>
public sealed record FeedbackSummaryQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackSummary item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackSummary> Apply(List<FeedbackSummary> items)
    {
        var matched = Collections.Filter(items, (FeedbackSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback summary store.</summary>
public static class FeedbackSummaryStore
{
    public static IFeedbackSummaryStore NewMemoryFeedbackSummaryStore()
    {
        return new MemoryFeedbackSummaryStore(new Dictionary<string, FeedbackSummary>(), new List<string>());
    }

    public static IFeedbackSummaryStore NewCachedFeedbackSummaryStore(MemoryFeedbackSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackSummaryStore(inner, FeedbackSummaryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FeedbackSummary> FindFeedbackSummaries(IFeedbackSummaryStore store, FeedbackSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
