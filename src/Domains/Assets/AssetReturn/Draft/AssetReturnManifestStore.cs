namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return manifests by code.</summary>
public interface IAssetReturnManifestStore
{
    AssetReturnManifest? Find(string code);
    void Save(AssetReturnManifest item);
    bool Remove(string code);
    List<AssetReturnManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnManifestStore(Dictionary<string, AssetReturnManifest> items, List<string> order) : IAssetReturnManifestStore
{
    public Dictionary<string, AssetReturnManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnManifest item)
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

    public List<AssetReturnManifest> List()
    {
        var outValue = new List<AssetReturnManifest>();
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
public sealed class CachedAssetReturnManifestStore(MemoryAssetReturnManifestStore inner, AssetReturnManifestCache cache, int hits, int misses) : IAssetReturnManifestStore
{
    public MemoryAssetReturnManifestStore Inner { get; set; } = inner;
    public AssetReturnManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnManifest? Find(string code)
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

    public void Save(AssetReturnManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return manifests. Empty fields match all.</summary>
public sealed record AssetReturnManifestQuery(string Label, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnManifest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<AssetReturnManifest> Apply(List<AssetReturnManifest> items)
    {
        var matched = Collections.Filter(items, (AssetReturnManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return manifest store.</summary>
public static class AssetReturnManifestStore
{
    public static IAssetReturnManifestStore NewMemoryAssetReturnManifestStore()
    {
        return new MemoryAssetReturnManifestStore(new Dictionary<string, AssetReturnManifest>(), new List<string>());
    }

    public static IAssetReturnManifestStore NewCachedAssetReturnManifestStore(MemoryAssetReturnManifestStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnManifestStore(inner, AssetReturnManifestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssetReturnManifest> FindAssetReturnManifests(IAssetReturnManifestStore store, AssetReturnManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
