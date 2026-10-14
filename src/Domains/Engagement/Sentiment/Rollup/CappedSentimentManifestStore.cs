namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped sentiment manifests by code.</summary>
public interface ICappedSentimentManifestStore
{
    CappedSentimentManifest? Find(string code);
    void Save(CappedSentimentManifest item);
    bool Remove(string code);
    List<CappedSentimentManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedSentimentManifestStore(Dictionary<string, CappedSentimentManifest> items, List<string> order) : ICappedSentimentManifestStore
{
    public Dictionary<string, CappedSentimentManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedSentimentManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedSentimentManifest item)
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

    public List<CappedSentimentManifest> List()
    {
        var outValue = new List<CappedSentimentManifest>();
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
public sealed class CachedCappedSentimentManifestStore(MemoryCappedSentimentManifestStore inner, CappedSentimentManifestCache cache, int hits, int misses) : ICappedSentimentManifestStore
{
    public MemoryCappedSentimentManifestStore Inner { get; set; } = inner;
    public CappedSentimentManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedSentimentManifest? Find(string code)
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

    public void Save(CappedSentimentManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedSentimentManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped sentiment manifests. Empty fields match all.</summary>
public sealed record CappedSentimentManifestQuery(string Channel, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(CappedSentimentManifest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedSentimentManifest> Apply(List<CappedSentimentManifest> items)
    {
        var matched = Collections.Filter(items, (CappedSentimentManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped sentiment manifest store.</summary>
public static class CappedSentimentManifestStore
{
    public static ICappedSentimentManifestStore NewMemoryCappedSentimentManifestStore()
    {
        return new MemoryCappedSentimentManifestStore(
            new Dictionary<string, CappedSentimentManifest>(),
            new List<string>());
    }

    public static ICappedSentimentManifestStore NewCachedCappedSentimentManifestStore(MemoryCappedSentimentManifestStore inner, int ttl, IClock clock)
    {
        return new CachedCappedSentimentManifestStore(
            inner,
            CappedSentimentManifestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedSentimentManifest> FindCappedSentimentManifests(ICappedSentimentManifestStore store, CappedSentimentManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
