namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive town hall tiers by code.</summary>
public interface IAdaptiveTownHallTierStore
{
    AdaptiveTownHallTier? Find(string code);
    void Save(AdaptiveTownHallTier item);
    bool Remove(string code);
    List<AdaptiveTownHallTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTownHallTierStore(Dictionary<string, AdaptiveTownHallTier> items, List<string> order) : IAdaptiveTownHallTierStore
{
    public Dictionary<string, AdaptiveTownHallTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTownHallTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTownHallTier item)
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

    public List<AdaptiveTownHallTier> List()
    {
        var outValue = new List<AdaptiveTownHallTier>();
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
public sealed class CachedAdaptiveTownHallTierStore(MemoryAdaptiveTownHallTierStore inner, AdaptiveTownHallTierCache cache, int hits, int misses) : IAdaptiveTownHallTierStore
{
    public MemoryAdaptiveTownHallTierStore Inner { get; set; } = inner;
    public AdaptiveTownHallTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTownHallTier? Find(string code)
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

    public void Save(AdaptiveTownHallTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTownHallTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive town hall tiers. Empty fields match all.</summary>
public sealed record AdaptiveTownHallTierQuery(string Region, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTownHallTier item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveTownHallTier> Apply(List<AdaptiveTownHallTier> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTownHallTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive town hall tier store.</summary>
public static class AdaptiveTownHallTierStore
{
    public static IAdaptiveTownHallTierStore NewMemoryAdaptiveTownHallTierStore()
    {
        return new MemoryAdaptiveTownHallTierStore(
            new Dictionary<string, AdaptiveTownHallTier>(),
            new List<string>());
    }

    public static IAdaptiveTownHallTierStore NewCachedAdaptiveTownHallTierStore(MemoryAdaptiveTownHallTierStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTownHallTierStore(inner, AdaptiveTownHallTierCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdaptiveTownHallTier> FindAdaptiveTownHallTiers(IAdaptiveTownHallTierStore store, AdaptiveTownHallTierQuery query)
    {
        return query.Apply(store.List());
    }
}
