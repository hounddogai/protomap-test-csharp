namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict asset return tiers by code.</summary>
public interface IStrictAssetReturnTierStore
{
    StrictAssetReturnTier? Find(string code);
    void Save(StrictAssetReturnTier item);
    bool Remove(string code);
    List<StrictAssetReturnTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAssetReturnTierStore(Dictionary<string, StrictAssetReturnTier> items, List<string> order) : IStrictAssetReturnTierStore
{
    public Dictionary<string, StrictAssetReturnTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAssetReturnTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAssetReturnTier item)
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

    public List<StrictAssetReturnTier> List()
    {
        var outValue = new List<StrictAssetReturnTier>();
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
public sealed class CachedStrictAssetReturnTierStore(MemoryStrictAssetReturnTierStore inner, StrictAssetReturnTierCache cache, int hits, int misses) : IStrictAssetReturnTierStore
{
    public MemoryStrictAssetReturnTierStore Inner { get; set; } = inner;
    public StrictAssetReturnTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAssetReturnTier? Find(string code)
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

    public void Save(StrictAssetReturnTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAssetReturnTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict asset return tiers. Empty fields match all.</summary>
public sealed record StrictAssetReturnTierQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAssetReturnTier item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAssetReturnTier> Apply(List<StrictAssetReturnTier> items)
    {
        var matched = Collections.Filter(items, (StrictAssetReturnTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict asset return tier store.</summary>
public static class StrictAssetReturnTierStore
{
    public static IStrictAssetReturnTierStore NewMemoryStrictAssetReturnTierStore()
    {
        return new MemoryStrictAssetReturnTierStore(
            new Dictionary<string, StrictAssetReturnTier>(),
            new List<string>());
    }

    public static IStrictAssetReturnTierStore NewCachedStrictAssetReturnTierStore(MemoryStrictAssetReturnTierStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAssetReturnTierStore(inner, StrictAssetReturnTierCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<StrictAssetReturnTier> FindStrictAssetReturnTiers(IStrictAssetReturnTierStore store, StrictAssetReturnTierQuery query)
    {
        return query.Apply(store.List());
    }
}
