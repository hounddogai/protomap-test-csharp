namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return baselines by code.</summary>
public interface IAssetReturnBaselineStore
{
    AssetReturnBaseline? Find(string code);
    void Save(AssetReturnBaseline item);
    bool Remove(string code);
    List<AssetReturnBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnBaselineStore(Dictionary<string, AssetReturnBaseline> items, List<string> order) : IAssetReturnBaselineStore
{
    public Dictionary<string, AssetReturnBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnBaseline item)
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

    public List<AssetReturnBaseline> List()
    {
        var outValue = new List<AssetReturnBaseline>();
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
public sealed class CachedAssetReturnBaselineStore(MemoryAssetReturnBaselineStore inner, AssetReturnBaselineCache cache, int hits, int misses) : IAssetReturnBaselineStore
{
    public MemoryAssetReturnBaselineStore Inner { get; set; } = inner;
    public AssetReturnBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnBaseline? Find(string code)
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

    public void Save(AssetReturnBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return baselines. Empty fields match all.</summary>
public sealed record AssetReturnBaselineQuery(string Department, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnBaseline item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<AssetReturnBaseline> Apply(List<AssetReturnBaseline> items)
    {
        var matched = Collections.Filter(items, (AssetReturnBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return baseline store.</summary>
public static class AssetReturnBaselineStore
{
    public static IAssetReturnBaselineStore NewMemoryAssetReturnBaselineStore()
    {
        return new MemoryAssetReturnBaselineStore(new Dictionary<string, AssetReturnBaseline>(), new List<string>());
    }

    public static IAssetReturnBaselineStore NewCachedAssetReturnBaselineStore(MemoryAssetReturnBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnBaselineStore(inner, AssetReturnBaselineCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AssetReturnBaseline> FindAssetReturnBaselines(IAssetReturnBaselineStore store, AssetReturnBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
