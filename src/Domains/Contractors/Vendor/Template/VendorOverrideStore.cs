namespace Acme.Workforce.Contractors.Vendor.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor overrides by code.</summary>
public interface IVendorOverrideStore
{
    VendorOverride? Find(string code);
    void Save(VendorOverride item);
    bool Remove(string code);
    List<VendorOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorOverrideStore(Dictionary<string, VendorOverride> items, List<string> order) : IVendorOverrideStore
{
    public Dictionary<string, VendorOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorOverride item)
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

    public List<VendorOverride> List()
    {
        var outValue = new List<VendorOverride>();
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
public sealed class CachedVendorOverrideStore(MemoryVendorOverrideStore inner, VendorOverrideCache cache, int hits, int misses) : IVendorOverrideStore
{
    public MemoryVendorOverrideStore Inner { get; set; } = inner;
    public VendorOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorOverride? Find(string code)
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

    public void Save(VendorOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor overrides. Empty fields match all.</summary>
public sealed record VendorOverrideQuery(string Region, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(VendorOverride item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<VendorOverride> Apply(List<VendorOverride> items)
    {
        var matched = Collections.Filter(items, (VendorOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor override store.</summary>
public static class VendorOverrideStore
{
    public static IVendorOverrideStore NewMemoryVendorOverrideStore()
    {
        return new MemoryVendorOverrideStore(new Dictionary<string, VendorOverride>(), new List<string>());
    }

    public static IVendorOverrideStore NewCachedVendorOverrideStore(MemoryVendorOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedVendorOverrideStore(inner, VendorOverrideCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VendorOverride> FindVendorOverrides(IVendorOverrideStore store, VendorOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
