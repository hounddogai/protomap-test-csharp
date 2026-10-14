namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted sentiment manifests by code.</summary>
public interface IWeightedSentimentManifestStore
{
    WeightedSentimentManifest? Find(string code);
    void Save(WeightedSentimentManifest item);
    bool Remove(string code);
    List<WeightedSentimentManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedSentimentManifestStore(Dictionary<string, WeightedSentimentManifest> items, List<string> order) : IWeightedSentimentManifestStore
{
    public Dictionary<string, WeightedSentimentManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedSentimentManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedSentimentManifest item)
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

    public List<WeightedSentimentManifest> List()
    {
        var outValue = new List<WeightedSentimentManifest>();
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
public sealed class CachedWeightedSentimentManifestStore(MemoryWeightedSentimentManifestStore inner, WeightedSentimentManifestCache cache, int hits, int misses) : IWeightedSentimentManifestStore
{
    public MemoryWeightedSentimentManifestStore Inner { get; set; } = inner;
    public WeightedSentimentManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedSentimentManifest? Find(string code)
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

    public void Save(WeightedSentimentManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedSentimentManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted sentiment manifests. Empty fields match all.</summary>
public sealed record WeightedSentimentManifestQuery(string Department, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedSentimentManifest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<WeightedSentimentManifest> Apply(List<WeightedSentimentManifest> items)
    {
        var matched = Collections.Filter(items, (WeightedSentimentManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted sentiment manifest store.</summary>
public static class WeightedSentimentManifestStore
{
    public static IWeightedSentimentManifestStore NewMemoryWeightedSentimentManifestStore()
    {
        return new MemoryWeightedSentimentManifestStore(
            new Dictionary<string, WeightedSentimentManifest>(),
            new List<string>());
    }

    public static IWeightedSentimentManifestStore NewCachedWeightedSentimentManifestStore(MemoryWeightedSentimentManifestStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedSentimentManifestStore(
            inner,
            WeightedSentimentManifestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedSentimentManifest> FindWeightedSentimentManifests(IWeightedSentimentManifestStore store, WeightedSentimentManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
