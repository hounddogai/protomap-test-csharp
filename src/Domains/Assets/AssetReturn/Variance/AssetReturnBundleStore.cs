namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return bundles by code.</summary>
public interface IAssetReturnBundleStore
{
    AssetReturnBundle? Find(string code);
    void Save(AssetReturnBundle item);
    bool Remove(string code);
    List<AssetReturnBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnBundleStore(Dictionary<string, AssetReturnBundle> items, List<string> order) : IAssetReturnBundleStore
{
    public Dictionary<string, AssetReturnBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnBundle item)
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

    public List<AssetReturnBundle> List()
    {
        var outValue = new List<AssetReturnBundle>();
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
public sealed class CachedAssetReturnBundleStore(MemoryAssetReturnBundleStore inner, AssetReturnBundleCache cache, int hits, int misses) : IAssetReturnBundleStore
{
    public MemoryAssetReturnBundleStore Inner { get; set; } = inner;
    public AssetReturnBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnBundle? Find(string code)
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

    public void Save(AssetReturnBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return bundles. Empty fields match all.</summary>
public sealed record AssetReturnBundleQuery(string Department, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnBundle item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<AssetReturnBundle> Apply(List<AssetReturnBundle> items)
    {
        var matched = Collections.Filter(items, (AssetReturnBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return bundle store.</summary>
public static class AssetReturnBundleStore
{
    public static IAssetReturnBundleStore NewMemoryAssetReturnBundleStore()
    {
        return new MemoryAssetReturnBundleStore(new Dictionary<string, AssetReturnBundle>(), new List<string>());
    }

    public static IAssetReturnBundleStore NewCachedAssetReturnBundleStore(MemoryAssetReturnBundleStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnBundleStore(inner, AssetReturnBundleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssetReturnBundle> FindAssetReturnBundles(IAssetReturnBundleStore store, AssetReturnBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
