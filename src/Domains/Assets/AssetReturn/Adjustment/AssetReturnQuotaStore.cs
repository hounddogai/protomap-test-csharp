namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return quotas by code.</summary>
public interface IAssetReturnQuotaStore
{
    AssetReturnQuota? Find(string code);
    void Save(AssetReturnQuota item);
    bool Remove(string code);
    List<AssetReturnQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnQuotaStore(Dictionary<string, AssetReturnQuota> items, List<string> order) : IAssetReturnQuotaStore
{
    public Dictionary<string, AssetReturnQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnQuota item)
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

    public List<AssetReturnQuota> List()
    {
        var outValue = new List<AssetReturnQuota>();
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
public sealed class CachedAssetReturnQuotaStore(MemoryAssetReturnQuotaStore inner, AssetReturnQuotaCache cache, int hits, int misses) : IAssetReturnQuotaStore
{
    public MemoryAssetReturnQuotaStore Inner { get; set; } = inner;
    public AssetReturnQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnQuota? Find(string code)
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

    public void Save(AssetReturnQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return quotas. Empty fields match all.</summary>
public sealed record AssetReturnQuotaQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnQuota item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnQuota> Apply(List<AssetReturnQuota> items)
    {
        var matched = Collections.Filter(items, (AssetReturnQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return quota store.</summary>
public static class AssetReturnQuotaStore
{
    public static IAssetReturnQuotaStore NewMemoryAssetReturnQuotaStore()
    {
        return new MemoryAssetReturnQuotaStore(new Dictionary<string, AssetReturnQuota>(), new List<string>());
    }

    public static IAssetReturnQuotaStore NewCachedAssetReturnQuotaStore(MemoryAssetReturnQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnQuotaStore(inner, AssetReturnQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AssetReturnQuota> FindAssetReturnQuotas(IAssetReturnQuotaStore store, AssetReturnQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
