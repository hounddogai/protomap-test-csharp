namespace Acme.Workforce.Contractors.Vendor.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor bundles by code.</summary>
public interface IVendorBundleStore
{
    VendorBundle? Find(string code);
    void Save(VendorBundle item);
    bool Remove(string code);
    List<VendorBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorBundleStore(Dictionary<string, VendorBundle> items, List<string> order) : IVendorBundleStore
{
    public Dictionary<string, VendorBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorBundle item)
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

    public List<VendorBundle> List()
    {
        var outValue = new List<VendorBundle>();
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
public sealed class CachedVendorBundleStore(MemoryVendorBundleStore inner, VendorBundleCache cache, int hits, int misses) : IVendorBundleStore
{
    public MemoryVendorBundleStore Inner { get; set; } = inner;
    public VendorBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorBundle? Find(string code)
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

    public void Save(VendorBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor bundles. Empty fields match all.</summary>
public sealed record VendorBundleQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(VendorBundle item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorBundle> Apply(List<VendorBundle> items)
    {
        var matched = Collections.Filter(items, (VendorBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor bundle store.</summary>
public static class VendorBundleStore
{
    public static IVendorBundleStore NewMemoryVendorBundleStore()
    {
        return new MemoryVendorBundleStore(new Dictionary<string, VendorBundle>(), new List<string>());
    }

    public static IVendorBundleStore NewCachedVendorBundleStore(MemoryVendorBundleStore inner, int ttl, IClock clock)
    {
        return new CachedVendorBundleStore(inner, VendorBundleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<VendorBundle> FindVendorBundles(IVendorBundleStore store, VendorBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
