namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return scores by code.</summary>
public interface IAssetReturnScoreStore
{
    AssetReturnScore? Find(string code);
    void Save(AssetReturnScore item);
    bool Remove(string code);
    List<AssetReturnScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnScoreStore(Dictionary<string, AssetReturnScore> items, List<string> order) : IAssetReturnScoreStore
{
    public Dictionary<string, AssetReturnScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnScore item)
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

    public List<AssetReturnScore> List()
    {
        var outValue = new List<AssetReturnScore>();
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
public sealed class CachedAssetReturnScoreStore(MemoryAssetReturnScoreStore inner, AssetReturnScoreCache cache, int hits, int misses) : IAssetReturnScoreStore
{
    public MemoryAssetReturnScoreStore Inner { get; set; } = inner;
    public AssetReturnScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnScore? Find(string code)
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

    public void Save(AssetReturnScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return scores. Empty fields match all.</summary>
public sealed record AssetReturnScoreQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnScore> Apply(List<AssetReturnScore> items)
    {
        var matched = Collections.Filter(items, (AssetReturnScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return score store.</summary>
public static class AssetReturnScoreStore
{
    public static IAssetReturnScoreStore NewMemoryAssetReturnScoreStore()
    {
        return new MemoryAssetReturnScoreStore(new Dictionary<string, AssetReturnScore>(), new List<string>());
    }

    public static IAssetReturnScoreStore NewCachedAssetReturnScoreStore(MemoryAssetReturnScoreStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnScoreStore(inner, AssetReturnScoreCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssetReturnScore> FindAssetReturnScores(IAssetReturnScoreStore store, AssetReturnScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
