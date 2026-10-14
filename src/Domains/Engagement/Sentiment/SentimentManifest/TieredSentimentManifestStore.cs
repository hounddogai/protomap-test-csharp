namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered sentiment manifests by code.</summary>
public interface ITieredSentimentManifestStore
{
    TieredSentimentManifest? Find(string code);
    void Save(TieredSentimentManifest item);
    bool Remove(string code);
    List<TieredSentimentManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredSentimentManifestStore(Dictionary<string, TieredSentimentManifest> items, List<string> order) : ITieredSentimentManifestStore
{
    public Dictionary<string, TieredSentimentManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredSentimentManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredSentimentManifest item)
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

    public List<TieredSentimentManifest> List()
    {
        var outValue = new List<TieredSentimentManifest>();
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
public sealed class CachedTieredSentimentManifestStore(MemoryTieredSentimentManifestStore inner, TieredSentimentManifestCache cache, int hits, int misses) : ITieredSentimentManifestStore
{
    public MemoryTieredSentimentManifestStore Inner { get; set; } = inner;
    public TieredSentimentManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredSentimentManifest? Find(string code)
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

    public void Save(TieredSentimentManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredSentimentManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered sentiment manifests. Empty fields match all.</summary>
public sealed record TieredSentimentManifestQuery(string Locale, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredSentimentManifest item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<TieredSentimentManifest> Apply(List<TieredSentimentManifest> items)
    {
        var matched = Collections.Filter(items, (TieredSentimentManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered sentiment manifest store.</summary>
public static class TieredSentimentManifestStore
{
    public static ITieredSentimentManifestStore NewMemoryTieredSentimentManifestStore()
    {
        return new MemoryTieredSentimentManifestStore(
            new Dictionary<string, TieredSentimentManifest>(),
            new List<string>());
    }

    public static ITieredSentimentManifestStore NewCachedTieredSentimentManifestStore(MemoryTieredSentimentManifestStore inner, int ttl, IClock clock)
    {
        return new CachedTieredSentimentManifestStore(
            inner,
            TieredSentimentManifestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TieredSentimentManifest> FindTieredSentimentManifests(ITieredSentimentManifestStore store, TieredSentimentManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
