namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop tiers by code.</summary>
public interface ILaptopTierStore
{
    LaptopTier? Find(string code);
    void Save(LaptopTier item);
    bool Remove(string code);
    List<LaptopTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopTierStore(Dictionary<string, LaptopTier> items, List<string> order) : ILaptopTierStore
{
    public Dictionary<string, LaptopTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopTier item)
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

    public List<LaptopTier> List()
    {
        var outValue = new List<LaptopTier>();
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
public sealed class CachedLaptopTierStore(MemoryLaptopTierStore inner, LaptopTierCache cache, int hits, int misses) : ILaptopTierStore
{
    public MemoryLaptopTierStore Inner { get; set; } = inner;
    public LaptopTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopTier? Find(string code)
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

    public void Save(LaptopTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop tiers. Empty fields match all.</summary>
public sealed record LaptopTierQuery(string Region, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopTier item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopTier> Apply(List<LaptopTier> items)
    {
        var matched = Collections.Filter(items, (LaptopTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop tier store.</summary>
public static class LaptopTierStore
{
    public static ILaptopTierStore NewMemoryLaptopTierStore()
    {
        return new MemoryLaptopTierStore(new Dictionary<string, LaptopTier>(), new List<string>());
    }

    public static ILaptopTierStore NewCachedLaptopTierStore(MemoryLaptopTierStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopTierStore(inner, LaptopTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LaptopTier> FindLaptopTiers(ILaptopTierStore store, LaptopTierQuery query)
    {
        return query.Apply(store.List());
    }
}
