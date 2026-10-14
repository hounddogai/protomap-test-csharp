namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback sentiment preferences by code.</summary>
public interface IFallbackSentimentPreferenceStore
{
    FallbackSentimentPreference? Find(string code);
    void Save(FallbackSentimentPreference item);
    bool Remove(string code);
    List<FallbackSentimentPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackSentimentPreferenceStore(Dictionary<string, FallbackSentimentPreference> items, List<string> order) : IFallbackSentimentPreferenceStore
{
    public Dictionary<string, FallbackSentimentPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackSentimentPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackSentimentPreference item)
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

    public List<FallbackSentimentPreference> List()
    {
        var outValue = new List<FallbackSentimentPreference>();
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
public sealed class CachedFallbackSentimentPreferenceStore(MemoryFallbackSentimentPreferenceStore inner, FallbackSentimentPreferenceCache cache, int hits, int misses) : IFallbackSentimentPreferenceStore
{
    public MemoryFallbackSentimentPreferenceStore Inner { get; set; } = inner;
    public FallbackSentimentPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackSentimentPreference? Find(string code)
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

    public void Save(FallbackSentimentPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackSentimentPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback sentiment preferences. Empty fields match all.</summary>
public sealed record FallbackSentimentPreferenceQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackSentimentPreference item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<FallbackSentimentPreference> Apply(List<FallbackSentimentPreference> items)
    {
        var matched = Collections.Filter(items, (FallbackSentimentPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback sentiment preference store.</summary>
public static class FallbackSentimentPreferenceStore
{
    public static IFallbackSentimentPreferenceStore NewMemoryFallbackSentimentPreferenceStore()
    {
        return new MemoryFallbackSentimentPreferenceStore(
            new Dictionary<string, FallbackSentimentPreference>(),
            new List<string>());
    }

    public static IFallbackSentimentPreferenceStore NewCachedFallbackSentimentPreferenceStore(MemoryFallbackSentimentPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackSentimentPreferenceStore(
            inner,
            FallbackSentimentPreferenceCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackSentimentPreference> FindFallbackSentimentPreferences(IFallbackSentimentPreferenceStore store, FallbackSentimentPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
