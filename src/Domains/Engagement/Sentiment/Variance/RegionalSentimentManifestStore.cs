namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional sentiment manifests by code.</summary>
public interface IRegionalSentimentManifestStore
{
    RegionalSentimentManifest? Find(string code);
    void Save(RegionalSentimentManifest item);
    bool Remove(string code);
    List<RegionalSentimentManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalSentimentManifestStore(Dictionary<string, RegionalSentimentManifest> items, List<string> order) : IRegionalSentimentManifestStore
{
    public Dictionary<string, RegionalSentimentManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalSentimentManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalSentimentManifest item)
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

    public List<RegionalSentimentManifest> List()
    {
        var outValue = new List<RegionalSentimentManifest>();
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
public sealed class CachedRegionalSentimentManifestStore(MemoryRegionalSentimentManifestStore inner, RegionalSentimentManifestCache cache, int hits, int misses) : IRegionalSentimentManifestStore
{
    public MemoryRegionalSentimentManifestStore Inner { get; set; } = inner;
    public RegionalSentimentManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalSentimentManifest? Find(string code)
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

    public void Save(RegionalSentimentManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalSentimentManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional sentiment manifests. Empty fields match all.</summary>
public sealed record RegionalSentimentManifestQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalSentimentManifest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalSentimentManifest> Apply(List<RegionalSentimentManifest> items)
    {
        var matched = Collections.Filter(items, (RegionalSentimentManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional sentiment manifest store.</summary>
public static class RegionalSentimentManifestStore
{
    public static IRegionalSentimentManifestStore NewMemoryRegionalSentimentManifestStore()
    {
        return new MemoryRegionalSentimentManifestStore(
            new Dictionary<string, RegionalSentimentManifest>(),
            new List<string>());
    }

    public static IRegionalSentimentManifestStore NewCachedRegionalSentimentManifestStore(MemoryRegionalSentimentManifestStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalSentimentManifestStore(
            inner,
            RegionalSentimentManifestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RegionalSentimentManifest> FindRegionalSentimentManifests(IRegionalSentimentManifestStore store, RegionalSentimentManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
