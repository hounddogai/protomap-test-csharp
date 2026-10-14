namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment variances by code.</summary>
public interface ISentimentVarianceStore
{
    SentimentVariance? Find(string code);
    void Save(SentimentVariance item);
    bool Remove(string code);
    List<SentimentVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentVarianceStore(Dictionary<string, SentimentVariance> items, List<string> order) : ISentimentVarianceStore
{
    public Dictionary<string, SentimentVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentVariance item)
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

    public List<SentimentVariance> List()
    {
        var outValue = new List<SentimentVariance>();
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
public sealed class CachedSentimentVarianceStore(MemorySentimentVarianceStore inner, SentimentVarianceCache cache, int hits, int misses) : ISentimentVarianceStore
{
    public MemorySentimentVarianceStore Inner { get; set; } = inner;
    public SentimentVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentVariance? Find(string code)
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

    public void Save(SentimentVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment variances. Empty fields match all.</summary>
public sealed record SentimentVarianceQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentVariance item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentVariance> Apply(List<SentimentVariance> items)
    {
        var matched = Collections.Filter(items, (SentimentVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment variance store.</summary>
public static class SentimentVarianceStore
{
    public static ISentimentVarianceStore NewMemorySentimentVarianceStore()
    {
        return new MemorySentimentVarianceStore(new Dictionary<string, SentimentVariance>(), new List<string>());
    }

    public static ISentimentVarianceStore NewCachedSentimentVarianceStore(MemorySentimentVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentVarianceStore(inner, SentimentVarianceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SentimentVariance> FindSentimentVariances(ISentimentVarianceStore store, SentimentVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
