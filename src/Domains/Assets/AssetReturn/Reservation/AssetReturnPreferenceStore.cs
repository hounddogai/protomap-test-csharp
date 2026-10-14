namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return preferences by code.</summary>
public interface IAssetReturnPreferenceStore
{
    AssetReturnPreference? Find(string code);
    void Save(AssetReturnPreference item);
    bool Remove(string code);
    List<AssetReturnPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnPreferenceStore(Dictionary<string, AssetReturnPreference> items, List<string> order) : IAssetReturnPreferenceStore
{
    public Dictionary<string, AssetReturnPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnPreference item)
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

    public List<AssetReturnPreference> List()
    {
        var outValue = new List<AssetReturnPreference>();
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
public sealed class CachedAssetReturnPreferenceStore(MemoryAssetReturnPreferenceStore inner, AssetReturnPreferenceCache cache, int hits, int misses) : IAssetReturnPreferenceStore
{
    public MemoryAssetReturnPreferenceStore Inner { get; set; } = inner;
    public AssetReturnPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnPreference? Find(string code)
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

    public void Save(AssetReturnPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return preferences. Empty fields match all.</summary>
public sealed record AssetReturnPreferenceQuery(string Label, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnPreference item)
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

    public List<AssetReturnPreference> Apply(List<AssetReturnPreference> items)
    {
        var matched = Collections.Filter(items, (AssetReturnPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return preference store.</summary>
public static class AssetReturnPreferenceStore
{
    public static IAssetReturnPreferenceStore NewMemoryAssetReturnPreferenceStore()
    {
        return new MemoryAssetReturnPreferenceStore(
            new Dictionary<string, AssetReturnPreference>(),
            new List<string>());
    }

    public static IAssetReturnPreferenceStore NewCachedAssetReturnPreferenceStore(MemoryAssetReturnPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnPreferenceStore(inner, AssetReturnPreferenceCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssetReturnPreference> FindAssetReturnPreferences(IAssetReturnPreferenceStore store, AssetReturnPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
