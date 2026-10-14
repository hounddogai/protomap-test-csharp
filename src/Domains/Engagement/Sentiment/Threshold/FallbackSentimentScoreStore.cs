namespace Acme.Workforce.Engagement.Sentiment.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback sentiment scores by code.</summary>
public interface IFallbackSentimentScoreStore
{
    FallbackSentimentScore? Find(string code);
    void Save(FallbackSentimentScore item);
    bool Remove(string code);
    List<FallbackSentimentScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackSentimentScoreStore(Dictionary<string, FallbackSentimentScore> items, List<string> order) : IFallbackSentimentScoreStore
{
    public Dictionary<string, FallbackSentimentScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackSentimentScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackSentimentScore item)
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

    public List<FallbackSentimentScore> List()
    {
        var outValue = new List<FallbackSentimentScore>();
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
public sealed class CachedFallbackSentimentScoreStore(MemoryFallbackSentimentScoreStore inner, FallbackSentimentScoreCache cache, int hits, int misses) : IFallbackSentimentScoreStore
{
    public MemoryFallbackSentimentScoreStore Inner { get; set; } = inner;
    public FallbackSentimentScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackSentimentScore? Find(string code)
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

    public void Save(FallbackSentimentScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackSentimentScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback sentiment scores. Empty fields match all.</summary>
public sealed record FallbackSentimentScoreQuery(string Country, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackSentimentScore item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackSentimentScore> Apply(List<FallbackSentimentScore> items)
    {
        var matched = Collections.Filter(items, (FallbackSentimentScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback sentiment score store.</summary>
public static class FallbackSentimentScoreStore
{
    public static IFallbackSentimentScoreStore NewMemoryFallbackSentimentScoreStore()
    {
        return new MemoryFallbackSentimentScoreStore(
            new Dictionary<string, FallbackSentimentScore>(),
            new List<string>());
    }

    public static IFallbackSentimentScoreStore NewCachedFallbackSentimentScoreStore(MemoryFallbackSentimentScoreStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackSentimentScoreStore(
            inner,
            FallbackSentimentScoreCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackSentimentScore> FindFallbackSentimentScores(IFallbackSentimentScoreStore store, FallbackSentimentScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
