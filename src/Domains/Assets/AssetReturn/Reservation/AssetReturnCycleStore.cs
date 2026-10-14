namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return cycles by code.</summary>
public interface IAssetReturnCycleStore
{
    AssetReturnCycle? Find(string code);
    void Save(AssetReturnCycle item);
    bool Remove(string code);
    List<AssetReturnCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnCycleStore(Dictionary<string, AssetReturnCycle> items, List<string> order) : IAssetReturnCycleStore
{
    public Dictionary<string, AssetReturnCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnCycle item)
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

    public List<AssetReturnCycle> List()
    {
        var outValue = new List<AssetReturnCycle>();
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
public sealed class CachedAssetReturnCycleStore(MemoryAssetReturnCycleStore inner, AssetReturnCycleCache cache, int hits, int misses) : IAssetReturnCycleStore
{
    public MemoryAssetReturnCycleStore Inner { get; set; } = inner;
    public AssetReturnCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnCycle? Find(string code)
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

    public void Save(AssetReturnCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return cycles. Empty fields match all.</summary>
public sealed record AssetReturnCycleQuery(string Department, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnCycle item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<AssetReturnCycle> Apply(List<AssetReturnCycle> items)
    {
        var matched = Collections.Filter(items, (AssetReturnCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return cycle store.</summary>
public static class AssetReturnCycleStore
{
    public static IAssetReturnCycleStore NewMemoryAssetReturnCycleStore()
    {
        return new MemoryAssetReturnCycleStore(new Dictionary<string, AssetReturnCycle>(), new List<string>());
    }

    public static IAssetReturnCycleStore NewCachedAssetReturnCycleStore(MemoryAssetReturnCycleStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnCycleStore(inner, AssetReturnCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssetReturnCycle> FindAssetReturnCycles(IAssetReturnCycleStore store, AssetReturnCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
