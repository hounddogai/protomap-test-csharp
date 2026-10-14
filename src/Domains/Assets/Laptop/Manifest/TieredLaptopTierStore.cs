namespace Acme.Workforce.Assets.Laptop.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered laptop tiers by code.</summary>
public interface ITieredLaptopTierStore
{
    TieredLaptopTier? Find(string code);
    void Save(TieredLaptopTier item);
    bool Remove(string code);
    List<TieredLaptopTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredLaptopTierStore(Dictionary<string, TieredLaptopTier> items, List<string> order) : ITieredLaptopTierStore
{
    public Dictionary<string, TieredLaptopTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredLaptopTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredLaptopTier item)
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

    public List<TieredLaptopTier> List()
    {
        var outValue = new List<TieredLaptopTier>();
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
public sealed class CachedTieredLaptopTierStore(MemoryTieredLaptopTierStore inner, TieredLaptopTierCache cache, int hits, int misses) : ITieredLaptopTierStore
{
    public MemoryTieredLaptopTierStore Inner { get; set; } = inner;
    public TieredLaptopTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredLaptopTier? Find(string code)
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

    public void Save(TieredLaptopTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredLaptopTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered laptop tiers. Empty fields match all.</summary>
public sealed record TieredLaptopTierQuery(string Source, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredLaptopTier item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredLaptopTier> Apply(List<TieredLaptopTier> items)
    {
        var matched = Collections.Filter(items, (TieredLaptopTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered laptop tier store.</summary>
public static class TieredLaptopTierStore
{
    public static ITieredLaptopTierStore NewMemoryTieredLaptopTierStore()
    {
        return new MemoryTieredLaptopTierStore(new Dictionary<string, TieredLaptopTier>(), new List<string>());
    }

    public static ITieredLaptopTierStore NewCachedTieredLaptopTierStore(MemoryTieredLaptopTierStore inner, int ttl, IClock clock)
    {
        return new CachedTieredLaptopTierStore(inner, TieredLaptopTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TieredLaptopTier> FindTieredLaptopTiers(ITieredLaptopTierStore store, TieredLaptopTierQuery query)
    {
        return query.Apply(store.List());
    }
}
