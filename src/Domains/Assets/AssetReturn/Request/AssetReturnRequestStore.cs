namespace Acme.Workforce.Assets.AssetReturn.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return requests by code.</summary>
public interface IAssetReturnRequestStore
{
    AssetReturnRequest? Find(string code);
    void Save(AssetReturnRequest item);
    bool Remove(string code);
    List<AssetReturnRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnRequestStore(Dictionary<string, AssetReturnRequest> items, List<string> order) : IAssetReturnRequestStore
{
    public Dictionary<string, AssetReturnRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnRequest item)
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

    public List<AssetReturnRequest> List()
    {
        var outValue = new List<AssetReturnRequest>();
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
public sealed class CachedAssetReturnRequestStore(MemoryAssetReturnRequestStore inner, AssetReturnRequestCache cache, int hits, int misses) : IAssetReturnRequestStore
{
    public MemoryAssetReturnRequestStore Inner { get; set; } = inner;
    public AssetReturnRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnRequest? Find(string code)
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

    public void Save(AssetReturnRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return requests. Empty fields match all.</summary>
public sealed record AssetReturnRequestQuery(string Department, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnRequest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<AssetReturnRequest> Apply(List<AssetReturnRequest> items)
    {
        var matched = Collections.Filter(items, (AssetReturnRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return request store.</summary>
public static class AssetReturnRequestStore
{
    public static IAssetReturnRequestStore NewMemoryAssetReturnRequestStore()
    {
        return new MemoryAssetReturnRequestStore(new Dictionary<string, AssetReturnRequest>(), new List<string>());
    }

    public static IAssetReturnRequestStore NewCachedAssetReturnRequestStore(MemoryAssetReturnRequestStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnRequestStore(inner, AssetReturnRequestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssetReturnRequest> FindAssetReturnRequests(IAssetReturnRequestStore store, AssetReturnRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
