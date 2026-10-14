namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient sentiment estimates by code.</summary>
public interface ILenientSentimentEstimateStore
{
    LenientSentimentEstimate? Find(string code);
    void Save(LenientSentimentEstimate item);
    bool Remove(string code);
    List<LenientSentimentEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientSentimentEstimateStore(Dictionary<string, LenientSentimentEstimate> items, List<string> order) : ILenientSentimentEstimateStore
{
    public Dictionary<string, LenientSentimentEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientSentimentEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientSentimentEstimate item)
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

    public List<LenientSentimentEstimate> List()
    {
        var outValue = new List<LenientSentimentEstimate>();
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
public sealed class CachedLenientSentimentEstimateStore(MemoryLenientSentimentEstimateStore inner, LenientSentimentEstimateCache cache, int hits, int misses) : ILenientSentimentEstimateStore
{
    public MemoryLenientSentimentEstimateStore Inner { get; set; } = inner;
    public LenientSentimentEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientSentimentEstimate? Find(string code)
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

    public void Save(LenientSentimentEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientSentimentEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient sentiment estimates. Empty fields match all.</summary>
public sealed record LenientSentimentEstimateQuery(string Title, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientSentimentEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientSentimentEstimate> Apply(List<LenientSentimentEstimate> items)
    {
        var matched = Collections.Filter(items, (LenientSentimentEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient sentiment estimate store.</summary>
public static class LenientSentimentEstimateStore
{
    public static ILenientSentimentEstimateStore NewMemoryLenientSentimentEstimateStore()
    {
        return new MemoryLenientSentimentEstimateStore(
            new Dictionary<string, LenientSentimentEstimate>(),
            new List<string>());
    }

    public static ILenientSentimentEstimateStore NewCachedLenientSentimentEstimateStore(MemoryLenientSentimentEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedLenientSentimentEstimateStore(
            inner,
            LenientSentimentEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LenientSentimentEstimate> FindLenientSentimentEstimates(ILenientSentimentEstimateStore store, LenientSentimentEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
