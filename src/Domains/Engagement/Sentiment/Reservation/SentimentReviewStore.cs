namespace Acme.Workforce.Engagement.Sentiment.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment reviews by code.</summary>
public interface ISentimentReviewStore
{
    SentimentReview? Find(string code);
    void Save(SentimentReview item);
    bool Remove(string code);
    List<SentimentReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentReviewStore(Dictionary<string, SentimentReview> items, List<string> order) : ISentimentReviewStore
{
    public Dictionary<string, SentimentReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentReview item)
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

    public List<SentimentReview> List()
    {
        var outValue = new List<SentimentReview>();
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
public sealed class CachedSentimentReviewStore(MemorySentimentReviewStore inner, SentimentReviewCache cache, int hits, int misses) : ISentimentReviewStore
{
    public MemorySentimentReviewStore Inner { get; set; } = inner;
    public SentimentReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentReview? Find(string code)
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

    public void Save(SentimentReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment reviews. Empty fields match all.</summary>
public sealed record SentimentReviewQuery(string Locale, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentReview item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentReview> Apply(List<SentimentReview> items)
    {
        var matched = Collections.Filter(items, (SentimentReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment review store.</summary>
public static class SentimentReviewStore
{
    public static ISentimentReviewStore NewMemorySentimentReviewStore()
    {
        return new MemorySentimentReviewStore(new Dictionary<string, SentimentReview>(), new List<string>());
    }

    public static ISentimentReviewStore NewCachedSentimentReviewStore(MemorySentimentReviewStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentReviewStore(inner, SentimentReviewCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SentimentReview> FindSentimentReviews(ISentimentReviewStore store, SentimentReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
