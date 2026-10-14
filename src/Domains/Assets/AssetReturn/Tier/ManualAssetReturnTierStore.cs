namespace Acme.Workforce.Assets.AssetReturn.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual asset return tiers by code.</summary>
public interface IManualAssetReturnTierStore
{
    ManualAssetReturnTier? Find(string code);
    void Save(ManualAssetReturnTier item);
    bool Remove(string code);
    List<ManualAssetReturnTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualAssetReturnTierStore(Dictionary<string, ManualAssetReturnTier> items, List<string> order) : IManualAssetReturnTierStore
{
    public Dictionary<string, ManualAssetReturnTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualAssetReturnTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualAssetReturnTier item)
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

    public List<ManualAssetReturnTier> List()
    {
        var outValue = new List<ManualAssetReturnTier>();
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
public sealed class CachedManualAssetReturnTierStore(MemoryManualAssetReturnTierStore inner, ManualAssetReturnTierCache cache, int hits, int misses) : IManualAssetReturnTierStore
{
    public MemoryManualAssetReturnTierStore Inner { get; set; } = inner;
    public ManualAssetReturnTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualAssetReturnTier? Find(string code)
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

    public void Save(ManualAssetReturnTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualAssetReturnTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual asset return tiers. Empty fields match all.</summary>
public sealed record ManualAssetReturnTierQuery(string Title, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualAssetReturnTier item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualAssetReturnTier> Apply(List<ManualAssetReturnTier> items)
    {
        var matched = Collections.Filter(items, (ManualAssetReturnTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual asset return tier store.</summary>
public static class ManualAssetReturnTierStore
{
    public static IManualAssetReturnTierStore NewMemoryManualAssetReturnTierStore()
    {
        return new MemoryManualAssetReturnTierStore(
            new Dictionary<string, ManualAssetReturnTier>(),
            new List<string>());
    }

    public static IManualAssetReturnTierStore NewCachedManualAssetReturnTierStore(MemoryManualAssetReturnTierStore inner, int ttl, IClock clock)
    {
        return new CachedManualAssetReturnTierStore(
            inner,
            ManualAssetReturnTierCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ManualAssetReturnTier> FindManualAssetReturnTiers(IManualAssetReturnTierStore store, ManualAssetReturnTierQuery query)
    {
        return query.Apply(store.List());
    }
}
