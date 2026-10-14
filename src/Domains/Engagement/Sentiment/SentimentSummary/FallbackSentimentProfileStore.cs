namespace Acme.Workforce.Engagement.Sentiment.SentimentSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback sentiment profiles by code.</summary>
public interface IFallbackSentimentProfileStore
{
    FallbackSentimentProfile? Find(string code);
    void Save(FallbackSentimentProfile item);
    bool Remove(string code);
    List<FallbackSentimentProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackSentimentProfileStore(Dictionary<string, FallbackSentimentProfile> items, List<string> order) : IFallbackSentimentProfileStore
{
    public Dictionary<string, FallbackSentimentProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackSentimentProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackSentimentProfile item)
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

    public List<FallbackSentimentProfile> List()
    {
        var outValue = new List<FallbackSentimentProfile>();
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
public sealed class CachedFallbackSentimentProfileStore(MemoryFallbackSentimentProfileStore inner, FallbackSentimentProfileCache cache, int hits, int misses) : IFallbackSentimentProfileStore
{
    public MemoryFallbackSentimentProfileStore Inner { get; set; } = inner;
    public FallbackSentimentProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackSentimentProfile? Find(string code)
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

    public void Save(FallbackSentimentProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackSentimentProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback sentiment profiles. Empty fields match all.</summary>
public sealed record FallbackSentimentProfileQuery(string Title, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackSentimentProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackSentimentProfile> Apply(List<FallbackSentimentProfile> items)
    {
        var matched = Collections.Filter(items, (FallbackSentimentProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback sentiment profile store.</summary>
public static class FallbackSentimentProfileStore
{
    public static IFallbackSentimentProfileStore NewMemoryFallbackSentimentProfileStore()
    {
        return new MemoryFallbackSentimentProfileStore(
            new Dictionary<string, FallbackSentimentProfile>(),
            new List<string>());
    }

    public static IFallbackSentimentProfileStore NewCachedFallbackSentimentProfileStore(MemoryFallbackSentimentProfileStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackSentimentProfileStore(
            inner,
            FallbackSentimentProfileCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackSentimentProfile> FindFallbackSentimentProfiles(IFallbackSentimentProfileStore store, FallbackSentimentProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
