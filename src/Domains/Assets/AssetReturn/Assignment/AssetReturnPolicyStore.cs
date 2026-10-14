namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return policies by code.</summary>
public interface IAssetReturnPolicyStore
{
    AssetReturnPolicy? Find(string code);
    void Save(AssetReturnPolicy item);
    bool Remove(string code);
    List<AssetReturnPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnPolicyStore(Dictionary<string, AssetReturnPolicy> items, List<string> order) : IAssetReturnPolicyStore
{
    public Dictionary<string, AssetReturnPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnPolicy item)
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

    public List<AssetReturnPolicy> List()
    {
        var outValue = new List<AssetReturnPolicy>();
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
public sealed class CachedAssetReturnPolicyStore(MemoryAssetReturnPolicyStore inner, AssetReturnPolicyCache cache, int hits, int misses) : IAssetReturnPolicyStore
{
    public MemoryAssetReturnPolicyStore Inner { get; set; } = inner;
    public AssetReturnPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnPolicy? Find(string code)
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

    public void Save(AssetReturnPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return policies. Empty fields match all.</summary>
public sealed record AssetReturnPolicyQuery(string Label, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnPolicy item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnPolicy> Apply(List<AssetReturnPolicy> items)
    {
        var matched = Collections.Filter(items, (AssetReturnPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return policy store.</summary>
public static class AssetReturnPolicyStore
{
    public static IAssetReturnPolicyStore NewMemoryAssetReturnPolicyStore()
    {
        return new MemoryAssetReturnPolicyStore(new Dictionary<string, AssetReturnPolicy>(), new List<string>());
    }

    public static IAssetReturnPolicyStore NewCachedAssetReturnPolicyStore(MemoryAssetReturnPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnPolicyStore(inner, AssetReturnPolicyCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssetReturnPolicy> FindAssetReturnPolicies(IAssetReturnPolicyStore store, AssetReturnPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
