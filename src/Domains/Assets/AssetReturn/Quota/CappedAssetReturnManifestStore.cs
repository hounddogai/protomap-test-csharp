namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped asset return manifests by code.</summary>
public interface ICappedAssetReturnManifestStore
{
    CappedAssetReturnManifest? Find(string code);
    void Save(CappedAssetReturnManifest item);
    bool Remove(string code);
    List<CappedAssetReturnManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAssetReturnManifestStore(Dictionary<string, CappedAssetReturnManifest> items, List<string> order) : ICappedAssetReturnManifestStore
{
    public Dictionary<string, CappedAssetReturnManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAssetReturnManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAssetReturnManifest item)
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

    public List<CappedAssetReturnManifest> List()
    {
        var outValue = new List<CappedAssetReturnManifest>();
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
public sealed class CachedCappedAssetReturnManifestStore(MemoryCappedAssetReturnManifestStore inner, CappedAssetReturnManifestCache cache, int hits, int misses) : ICappedAssetReturnManifestStore
{
    public MemoryCappedAssetReturnManifestStore Inner { get; set; } = inner;
    public CappedAssetReturnManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAssetReturnManifest? Find(string code)
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

    public void Save(CappedAssetReturnManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAssetReturnManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped asset return manifests. Empty fields match all.</summary>
public sealed record CappedAssetReturnManifestQuery(string Region, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAssetReturnManifest item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAssetReturnManifest> Apply(List<CappedAssetReturnManifest> items)
    {
        var matched = Collections.Filter(items, (CappedAssetReturnManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped asset return manifest store.</summary>
public static class CappedAssetReturnManifestStore
{
    public static ICappedAssetReturnManifestStore NewMemoryCappedAssetReturnManifestStore()
    {
        return new MemoryCappedAssetReturnManifestStore(
            new Dictionary<string, CappedAssetReturnManifest>(),
            new List<string>());
    }

    public static ICappedAssetReturnManifestStore NewCachedCappedAssetReturnManifestStore(MemoryCappedAssetReturnManifestStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAssetReturnManifestStore(
            inner,
            CappedAssetReturnManifestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedAssetReturnManifest> FindCappedAssetReturnManifests(ICappedAssetReturnManifestStore store, CappedAssetReturnManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
