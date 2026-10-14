namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return overrides by code.</summary>
public interface IAssetReturnOverrideStore
{
    AssetReturnOverride? Find(string code);
    void Save(AssetReturnOverride item);
    bool Remove(string code);
    List<AssetReturnOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnOverrideStore(Dictionary<string, AssetReturnOverride> items, List<string> order) : IAssetReturnOverrideStore
{
    public Dictionary<string, AssetReturnOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnOverride item)
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

    public List<AssetReturnOverride> List()
    {
        var outValue = new List<AssetReturnOverride>();
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
public sealed class CachedAssetReturnOverrideStore(MemoryAssetReturnOverrideStore inner, AssetReturnOverrideCache cache, int hits, int misses) : IAssetReturnOverrideStore
{
    public MemoryAssetReturnOverrideStore Inner { get; set; } = inner;
    public AssetReturnOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnOverride? Find(string code)
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

    public void Save(AssetReturnOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return overrides. Empty fields match all.</summary>
public sealed record AssetReturnOverrideQuery(string Source, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnOverride item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnOverride> Apply(List<AssetReturnOverride> items)
    {
        var matched = Collections.Filter(items, (AssetReturnOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return override store.</summary>
public static class AssetReturnOverrideStore
{
    public static IAssetReturnOverrideStore NewMemoryAssetReturnOverrideStore()
    {
        return new MemoryAssetReturnOverrideStore(new Dictionary<string, AssetReturnOverride>(), new List<string>());
    }

    public static IAssetReturnOverrideStore NewCachedAssetReturnOverrideStore(MemoryAssetReturnOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnOverrideStore(inner, AssetReturnOverrideCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AssetReturnOverride> FindAssetReturnOverrides(IAssetReturnOverrideStore store, AssetReturnOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
