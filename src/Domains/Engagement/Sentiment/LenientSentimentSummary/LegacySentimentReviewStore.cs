namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy sentiment reviews by code.</summary>
public interface ILegacySentimentReviewStore
{
    LegacySentimentReview? Find(string code);
    void Save(LegacySentimentReview item);
    bool Remove(string code);
    List<LegacySentimentReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacySentimentReviewStore(Dictionary<string, LegacySentimentReview> items, List<string> order) : ILegacySentimentReviewStore
{
    public Dictionary<string, LegacySentimentReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacySentimentReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacySentimentReview item)
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

    public List<LegacySentimentReview> List()
    {
        var outValue = new List<LegacySentimentReview>();
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
public sealed class CachedLegacySentimentReviewStore(MemoryLegacySentimentReviewStore inner, LegacySentimentReviewCache cache, int hits, int misses) : ILegacySentimentReviewStore
{
    public MemoryLegacySentimentReviewStore Inner { get; set; } = inner;
    public LegacySentimentReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacySentimentReview? Find(string code)
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

    public void Save(LegacySentimentReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacySentimentReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy sentiment reviews. Empty fields match all.</summary>
public sealed record LegacySentimentReviewQuery(string Unit, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(LegacySentimentReview item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacySentimentReview> Apply(List<LegacySentimentReview> items)
    {
        var matched = Collections.Filter(items, (LegacySentimentReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy sentiment review store.</summary>
public static class LegacySentimentReviewStore
{
    public static ILegacySentimentReviewStore NewMemoryLegacySentimentReviewStore()
    {
        return new MemoryLegacySentimentReviewStore(
            new Dictionary<string, LegacySentimentReview>(),
            new List<string>());
    }

    public static ILegacySentimentReviewStore NewCachedLegacySentimentReviewStore(MemoryLegacySentimentReviewStore inner, int ttl, IClock clock)
    {
        return new CachedLegacySentimentReviewStore(inner, LegacySentimentReviewCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LegacySentimentReview> FindLegacySentimentReviews(ILegacySentimentReviewStore store, LegacySentimentReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
