namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback sentiment bundles by code.</summary>
public interface IFallbackSentimentBundleStore
{
    FallbackSentimentBundle? Find(string code);
    void Save(FallbackSentimentBundle item);
    bool Remove(string code);
    List<FallbackSentimentBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackSentimentBundleStore(Dictionary<string, FallbackSentimentBundle> items, List<string> order) : IFallbackSentimentBundleStore
{
    public Dictionary<string, FallbackSentimentBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackSentimentBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackSentimentBundle item)
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

    public List<FallbackSentimentBundle> List()
    {
        var outValue = new List<FallbackSentimentBundle>();
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
public sealed class CachedFallbackSentimentBundleStore(MemoryFallbackSentimentBundleStore inner, FallbackSentimentBundleCache cache, int hits, int misses) : IFallbackSentimentBundleStore
{
    public MemoryFallbackSentimentBundleStore Inner { get; set; } = inner;
    public FallbackSentimentBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackSentimentBundle? Find(string code)
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

    public void Save(FallbackSentimentBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackSentimentBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback sentiment bundles. Empty fields match all.</summary>
public sealed record FallbackSentimentBundleQuery(string Title, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackSentimentBundle item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackSentimentBundle> Apply(List<FallbackSentimentBundle> items)
    {
        var matched = Collections.Filter(items, (FallbackSentimentBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback sentiment bundle store.</summary>
public static class FallbackSentimentBundleStore
{
    public static IFallbackSentimentBundleStore NewMemoryFallbackSentimentBundleStore()
    {
        return new MemoryFallbackSentimentBundleStore(
            new Dictionary<string, FallbackSentimentBundle>(),
            new List<string>());
    }

    public static IFallbackSentimentBundleStore NewCachedFallbackSentimentBundleStore(MemoryFallbackSentimentBundleStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackSentimentBundleStore(
            inner,
            FallbackSentimentBundleCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackSentimentBundle> FindFallbackSentimentBundles(IFallbackSentimentBundleStore store, FallbackSentimentBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
