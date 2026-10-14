namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return profiles by code.</summary>
public interface IAssetReturnProfileStore
{
    AssetReturnProfile? Find(string code);
    void Save(AssetReturnProfile item);
    bool Remove(string code);
    List<AssetReturnProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnProfileStore(Dictionary<string, AssetReturnProfile> items, List<string> order) : IAssetReturnProfileStore
{
    public Dictionary<string, AssetReturnProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnProfile item)
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

    public List<AssetReturnProfile> List()
    {
        var outValue = new List<AssetReturnProfile>();
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
public sealed class CachedAssetReturnProfileStore(MemoryAssetReturnProfileStore inner, AssetReturnProfileCache cache, int hits, int misses) : IAssetReturnProfileStore
{
    public MemoryAssetReturnProfileStore Inner { get; set; } = inner;
    public AssetReturnProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnProfile? Find(string code)
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

    public void Save(AssetReturnProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return profiles. Empty fields match all.</summary>
public sealed record AssetReturnProfileQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnProfile> Apply(List<AssetReturnProfile> items)
    {
        var matched = Collections.Filter(items, (AssetReturnProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return profile store.</summary>
public static class AssetReturnProfileStore
{
    public static IAssetReturnProfileStore NewMemoryAssetReturnProfileStore()
    {
        return new MemoryAssetReturnProfileStore(new Dictionary<string, AssetReturnProfile>(), new List<string>());
    }

    public static IAssetReturnProfileStore NewCachedAssetReturnProfileStore(MemoryAssetReturnProfileStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnProfileStore(inner, AssetReturnProfileCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssetReturnProfile> FindAssetReturnProfiles(IAssetReturnProfileStore store, AssetReturnProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
