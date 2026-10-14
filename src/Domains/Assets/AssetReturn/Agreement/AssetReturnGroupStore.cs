namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return groups by code.</summary>
public interface IAssetReturnGroupStore
{
    AssetReturnGroup? Find(string code);
    void Save(AssetReturnGroup item);
    bool Remove(string code);
    List<AssetReturnGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnGroupStore(Dictionary<string, AssetReturnGroup> items, List<string> order) : IAssetReturnGroupStore
{
    public Dictionary<string, AssetReturnGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnGroup item)
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

    public List<AssetReturnGroup> List()
    {
        var outValue = new List<AssetReturnGroup>();
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
public sealed class CachedAssetReturnGroupStore(MemoryAssetReturnGroupStore inner, AssetReturnGroupCache cache, int hits, int misses) : IAssetReturnGroupStore
{
    public MemoryAssetReturnGroupStore Inner { get; set; } = inner;
    public AssetReturnGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnGroup? Find(string code)
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

    public void Save(AssetReturnGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return groups. Empty fields match all.</summary>
public sealed record AssetReturnGroupQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnGroup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnGroup> Apply(List<AssetReturnGroup> items)
    {
        var matched = Collections.Filter(items, (AssetReturnGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return group store.</summary>
public static class AssetReturnGroupStore
{
    public static IAssetReturnGroupStore NewMemoryAssetReturnGroupStore()
    {
        return new MemoryAssetReturnGroupStore(new Dictionary<string, AssetReturnGroup>(), new List<string>());
    }

    public static IAssetReturnGroupStore NewCachedAssetReturnGroupStore(MemoryAssetReturnGroupStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnGroupStore(inner, AssetReturnGroupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssetReturnGroup> FindAssetReturnGroups(IAssetReturnGroupStore store, AssetReturnGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
