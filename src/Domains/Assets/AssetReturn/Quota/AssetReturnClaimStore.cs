namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return claims by code.</summary>
public interface IAssetReturnClaimStore
{
    AssetReturnClaim? Find(string code);
    void Save(AssetReturnClaim item);
    bool Remove(string code);
    List<AssetReturnClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnClaimStore(Dictionary<string, AssetReturnClaim> items, List<string> order) : IAssetReturnClaimStore
{
    public Dictionary<string, AssetReturnClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnClaim item)
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

    public List<AssetReturnClaim> List()
    {
        var outValue = new List<AssetReturnClaim>();
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
public sealed class CachedAssetReturnClaimStore(MemoryAssetReturnClaimStore inner, AssetReturnClaimCache cache, int hits, int misses) : IAssetReturnClaimStore
{
    public MemoryAssetReturnClaimStore Inner { get; set; } = inner;
    public AssetReturnClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnClaim? Find(string code)
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

    public void Save(AssetReturnClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return claims. Empty fields match all.</summary>
public sealed record AssetReturnClaimQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnClaim item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<AssetReturnClaim> Apply(List<AssetReturnClaim> items)
    {
        var matched = Collections.Filter(items, (AssetReturnClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return claim store.</summary>
public static class AssetReturnClaimStore
{
    public static IAssetReturnClaimStore NewMemoryAssetReturnClaimStore()
    {
        return new MemoryAssetReturnClaimStore(new Dictionary<string, AssetReturnClaim>(), new List<string>());
    }

    public static IAssetReturnClaimStore NewCachedAssetReturnClaimStore(MemoryAssetReturnClaimStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnClaimStore(inner, AssetReturnClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AssetReturnClaim> FindAssetReturnClaims(IAssetReturnClaimStore store, AssetReturnClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
