namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return targets by code.</summary>
public interface IAssetReturnTargetStore
{
    AssetReturnTarget? Find(string code);
    void Save(AssetReturnTarget item);
    bool Remove(string code);
    List<AssetReturnTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnTargetStore(Dictionary<string, AssetReturnTarget> items, List<string> order) : IAssetReturnTargetStore
{
    public Dictionary<string, AssetReturnTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnTarget item)
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

    public List<AssetReturnTarget> List()
    {
        var outValue = new List<AssetReturnTarget>();
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
public sealed class CachedAssetReturnTargetStore(MemoryAssetReturnTargetStore inner, AssetReturnTargetCache cache, int hits, int misses) : IAssetReturnTargetStore
{
    public MemoryAssetReturnTargetStore Inner { get; set; } = inner;
    public AssetReturnTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnTarget? Find(string code)
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

    public void Save(AssetReturnTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return targets. Empty fields match all.</summary>
public sealed record AssetReturnTargetQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnTarget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<AssetReturnTarget> Apply(List<AssetReturnTarget> items)
    {
        var matched = Collections.Filter(items, (AssetReturnTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return target store.</summary>
public static class AssetReturnTargetStore
{
    public static IAssetReturnTargetStore NewMemoryAssetReturnTargetStore()
    {
        return new MemoryAssetReturnTargetStore(new Dictionary<string, AssetReturnTarget>(), new List<string>());
    }

    public static IAssetReturnTargetStore NewCachedAssetReturnTargetStore(MemoryAssetReturnTargetStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnTargetStore(inner, AssetReturnTargetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssetReturnTarget> FindAssetReturnTargets(IAssetReturnTargetStore store, AssetReturnTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
