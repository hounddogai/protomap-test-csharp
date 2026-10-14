namespace Acme.Workforce.Assets.Warranty.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty tiers by code.</summary>
public interface IWarrantyTierStore
{
    WarrantyTier? Find(string code);
    void Save(WarrantyTier item);
    bool Remove(string code);
    List<WarrantyTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyTierStore(Dictionary<string, WarrantyTier> items, List<string> order) : IWarrantyTierStore
{
    public Dictionary<string, WarrantyTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyTier item)
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

    public List<WarrantyTier> List()
    {
        var outValue = new List<WarrantyTier>();
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
public sealed class CachedWarrantyTierStore(MemoryWarrantyTierStore inner, WarrantyTierCache cache, int hits, int misses) : IWarrantyTierStore
{
    public MemoryWarrantyTierStore Inner { get; set; } = inner;
    public WarrantyTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyTier? Find(string code)
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

    public void Save(WarrantyTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty tiers. Empty fields match all.</summary>
public sealed record WarrantyTierQuery(string Source, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyTier item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<WarrantyTier> Apply(List<WarrantyTier> items)
    {
        var matched = Collections.Filter(items, (WarrantyTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty tier store.</summary>
public static class WarrantyTierStore
{
    public static IWarrantyTierStore NewMemoryWarrantyTierStore()
    {
        return new MemoryWarrantyTierStore(new Dictionary<string, WarrantyTier>(), new List<string>());
    }

    public static IWarrantyTierStore NewCachedWarrantyTierStore(MemoryWarrantyTierStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyTierStore(inner, WarrantyTierCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<WarrantyTier> FindWarrantyTiers(IWarrantyTierStore store, WarrantyTierQuery query)
    {
        return query.Apply(store.List());
    }
}
