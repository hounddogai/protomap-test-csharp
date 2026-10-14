namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return tiers by code.</summary>
public interface IAssetReturnTierStore
{
    AssetReturnTier? Find(string code);
    void Save(AssetReturnTier item);
    bool Remove(string code);
    List<AssetReturnTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnTierStore(Dictionary<string, AssetReturnTier> items, List<string> order) : IAssetReturnTierStore
{
    public Dictionary<string, AssetReturnTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnTier item)
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

    public List<AssetReturnTier> List()
    {
        var outValue = new List<AssetReturnTier>();
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
public sealed class CachedAssetReturnTierStore(MemoryAssetReturnTierStore inner, AssetReturnTierCache cache, int hits, int misses) : IAssetReturnTierStore
{
    public MemoryAssetReturnTierStore Inner { get; set; } = inner;
    public AssetReturnTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnTier? Find(string code)
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

    public void Save(AssetReturnTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return tiers. Empty fields match all.</summary>
public sealed record AssetReturnTierQuery(string Region, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnTier item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnTier> Apply(List<AssetReturnTier> items)
    {
        var matched = Collections.Filter(items, (AssetReturnTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return tier store.</summary>
public static class AssetReturnTierStore
{
    public static IAssetReturnTierStore NewMemoryAssetReturnTierStore()
    {
        return new MemoryAssetReturnTierStore(new Dictionary<string, AssetReturnTier>(), new List<string>());
    }

    public static IAssetReturnTierStore NewCachedAssetReturnTierStore(MemoryAssetReturnTierStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnTierStore(inner, AssetReturnTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssetReturnTier> FindAssetReturnTiers(IAssetReturnTierStore store, AssetReturnTierQuery query)
    {
        return query.Apply(store.List());
    }
}
