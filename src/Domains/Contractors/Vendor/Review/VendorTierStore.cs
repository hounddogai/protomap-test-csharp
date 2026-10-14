namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor tiers by code.</summary>
public interface IVendorTierStore
{
    VendorTier? Find(string code);
    void Save(VendorTier item);
    bool Remove(string code);
    List<VendorTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorTierStore(Dictionary<string, VendorTier> items, List<string> order) : IVendorTierStore
{
    public Dictionary<string, VendorTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorTier item)
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

    public List<VendorTier> List()
    {
        var outValue = new List<VendorTier>();
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
public sealed class CachedVendorTierStore(MemoryVendorTierStore inner, VendorTierCache cache, int hits, int misses) : IVendorTierStore
{
    public MemoryVendorTierStore Inner { get; set; } = inner;
    public VendorTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorTier? Find(string code)
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

    public void Save(VendorTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor tiers. Empty fields match all.</summary>
public sealed record VendorTierQuery(string Country, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(VendorTier item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<VendorTier> Apply(List<VendorTier> items)
    {
        var matched = Collections.Filter(items, (VendorTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor tier store.</summary>
public static class VendorTierStore
{
    public static IVendorTierStore NewMemoryVendorTierStore()
    {
        return new MemoryVendorTierStore(new Dictionary<string, VendorTier>(), new List<string>());
    }

    public static IVendorTierStore NewCachedVendorTierStore(MemoryVendorTierStore inner, int ttl, IClock clock)
    {
        return new CachedVendorTierStore(inner, VendorTierCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<VendorTier> FindVendorTiers(IVendorTierStore store, VendorTierQuery query)
    {
        return query.Apply(store.List());
    }
}
