namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return estimates by code.</summary>
public interface IAssetReturnEstimateStore
{
    AssetReturnEstimate? Find(string code);
    void Save(AssetReturnEstimate item);
    bool Remove(string code);
    List<AssetReturnEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnEstimateStore(Dictionary<string, AssetReturnEstimate> items, List<string> order) : IAssetReturnEstimateStore
{
    public Dictionary<string, AssetReturnEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnEstimate item)
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

    public List<AssetReturnEstimate> List()
    {
        var outValue = new List<AssetReturnEstimate>();
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
public sealed class CachedAssetReturnEstimateStore(MemoryAssetReturnEstimateStore inner, AssetReturnEstimateCache cache, int hits, int misses) : IAssetReturnEstimateStore
{
    public MemoryAssetReturnEstimateStore Inner { get; set; } = inner;
    public AssetReturnEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnEstimate? Find(string code)
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

    public void Save(AssetReturnEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return estimates. Empty fields match all.</summary>
public sealed record AssetReturnEstimateQuery(string Channel, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnEstimate item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnEstimate> Apply(List<AssetReturnEstimate> items)
    {
        var matched = Collections.Filter(items, (AssetReturnEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return estimate store.</summary>
public static class AssetReturnEstimateStore
{
    public static IAssetReturnEstimateStore NewMemoryAssetReturnEstimateStore()
    {
        return new MemoryAssetReturnEstimateStore(new Dictionary<string, AssetReturnEstimate>(), new List<string>());
    }

    public static IAssetReturnEstimateStore NewCachedAssetReturnEstimateStore(MemoryAssetReturnEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnEstimateStore(inner, AssetReturnEstimateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssetReturnEstimate> FindAssetReturnEstimates(IAssetReturnEstimateStore store, AssetReturnEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
