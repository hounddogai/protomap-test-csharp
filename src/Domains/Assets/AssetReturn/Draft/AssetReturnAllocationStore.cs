namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return allocations by code.</summary>
public interface IAssetReturnAllocationStore
{
    AssetReturnAllocation? Find(string code);
    void Save(AssetReturnAllocation item);
    bool Remove(string code);
    List<AssetReturnAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnAllocationStore(Dictionary<string, AssetReturnAllocation> items, List<string> order) : IAssetReturnAllocationStore
{
    public Dictionary<string, AssetReturnAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnAllocation item)
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

    public List<AssetReturnAllocation> List()
    {
        var outValue = new List<AssetReturnAllocation>();
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
public sealed class CachedAssetReturnAllocationStore(MemoryAssetReturnAllocationStore inner, AssetReturnAllocationCache cache, int hits, int misses) : IAssetReturnAllocationStore
{
    public MemoryAssetReturnAllocationStore Inner { get; set; } = inner;
    public AssetReturnAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnAllocation? Find(string code)
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

    public void Save(AssetReturnAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return allocations. Empty fields match all.</summary>
public sealed record AssetReturnAllocationQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnAllocation item)
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

    public List<AssetReturnAllocation> Apply(List<AssetReturnAllocation> items)
    {
        var matched = Collections.Filter(items, (AssetReturnAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return allocation store.</summary>
public static class AssetReturnAllocationStore
{
    public static IAssetReturnAllocationStore NewMemoryAssetReturnAllocationStore()
    {
        return new MemoryAssetReturnAllocationStore(
            new Dictionary<string, AssetReturnAllocation>(),
            new List<string>());
    }

    public static IAssetReturnAllocationStore NewCachedAssetReturnAllocationStore(MemoryAssetReturnAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnAllocationStore(
            inner,
            AssetReturnAllocationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AssetReturnAllocation> FindAssetReturnAllocations(IAssetReturnAllocationStore store, AssetReturnAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
