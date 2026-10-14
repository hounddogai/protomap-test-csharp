namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return adjustments by code.</summary>
public interface IAssetReturnAdjustmentStore
{
    AssetReturnAdjustment? Find(string code);
    void Save(AssetReturnAdjustment item);
    bool Remove(string code);
    List<AssetReturnAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnAdjustmentStore(Dictionary<string, AssetReturnAdjustment> items, List<string> order) : IAssetReturnAdjustmentStore
{
    public Dictionary<string, AssetReturnAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnAdjustment item)
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

    public List<AssetReturnAdjustment> List()
    {
        var outValue = new List<AssetReturnAdjustment>();
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
public sealed class CachedAssetReturnAdjustmentStore(MemoryAssetReturnAdjustmentStore inner, AssetReturnAdjustmentCache cache, int hits, int misses) : IAssetReturnAdjustmentStore
{
    public MemoryAssetReturnAdjustmentStore Inner { get; set; } = inner;
    public AssetReturnAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnAdjustment? Find(string code)
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

    public void Save(AssetReturnAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return adjustments. Empty fields match all.</summary>
public sealed record AssetReturnAdjustmentQuery(string Unit, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnAdjustment item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnAdjustment> Apply(List<AssetReturnAdjustment> items)
    {
        var matched = Collections.Filter(items, (AssetReturnAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return adjustment store.</summary>
public static class AssetReturnAdjustmentStore
{
    public static IAssetReturnAdjustmentStore NewMemoryAssetReturnAdjustmentStore()
    {
        return new MemoryAssetReturnAdjustmentStore(
            new Dictionary<string, AssetReturnAdjustment>(),
            new List<string>());
    }

    public static IAssetReturnAdjustmentStore NewCachedAssetReturnAdjustmentStore(MemoryAssetReturnAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnAdjustmentStore(inner, AssetReturnAdjustmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AssetReturnAdjustment> FindAssetReturnAdjustments(IAssetReturnAdjustmentStore store, AssetReturnAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
