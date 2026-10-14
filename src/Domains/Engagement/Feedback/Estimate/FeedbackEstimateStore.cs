namespace Acme.Workforce.Engagement.Feedback.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback estimates by code.</summary>
public interface IFeedbackEstimateStore
{
    FeedbackEstimate? Find(string code);
    void Save(FeedbackEstimate item);
    bool Remove(string code);
    List<FeedbackEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackEstimateStore(Dictionary<string, FeedbackEstimate> items, List<string> order) : IFeedbackEstimateStore
{
    public Dictionary<string, FeedbackEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackEstimate item)
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

    public List<FeedbackEstimate> List()
    {
        var outValue = new List<FeedbackEstimate>();
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
public sealed class CachedFeedbackEstimateStore(MemoryFeedbackEstimateStore inner, FeedbackEstimateCache cache, int hits, int misses) : IFeedbackEstimateStore
{
    public MemoryFeedbackEstimateStore Inner { get; set; } = inner;
    public FeedbackEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackEstimate? Find(string code)
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

    public void Save(FeedbackEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback estimates. Empty fields match all.</summary>
public sealed record FeedbackEstimateQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<FeedbackEstimate> Apply(List<FeedbackEstimate> items)
    {
        var matched = Collections.Filter(items, (FeedbackEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback estimate store.</summary>
public static class FeedbackEstimateStore
{
    public static IFeedbackEstimateStore NewMemoryFeedbackEstimateStore()
    {
        return new MemoryFeedbackEstimateStore(new Dictionary<string, FeedbackEstimate>(), new List<string>());
    }

    public static IFeedbackEstimateStore NewCachedFeedbackEstimateStore(MemoryFeedbackEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackEstimateStore(inner, FeedbackEstimateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FeedbackEstimate> FindFeedbackEstimates(IFeedbackEstimateStore store, FeedbackEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
