namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor windows by code.</summary>
public interface IVendorWindowStore
{
    VendorWindow? Find(string code);
    void Save(VendorWindow item);
    bool Remove(string code);
    List<VendorWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorWindowStore(Dictionary<string, VendorWindow> items, List<string> order) : IVendorWindowStore
{
    public Dictionary<string, VendorWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorWindow item)
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

    public List<VendorWindow> List()
    {
        var outValue = new List<VendorWindow>();
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
public sealed class CachedVendorWindowStore(MemoryVendorWindowStore inner, VendorWindowCache cache, int hits, int misses) : IVendorWindowStore
{
    public MemoryVendorWindowStore Inner { get; set; } = inner;
    public VendorWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorWindow? Find(string code)
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

    public void Save(VendorWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor windows. Empty fields match all.</summary>
public sealed record VendorWindowQuery(string Country, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(VendorWindow item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorWindow> Apply(List<VendorWindow> items)
    {
        var matched = Collections.Filter(items, (VendorWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor window store.</summary>
public static class VendorWindowStore
{
    public static IVendorWindowStore NewMemoryVendorWindowStore()
    {
        return new MemoryVendorWindowStore(new Dictionary<string, VendorWindow>(), new List<string>());
    }

    public static IVendorWindowStore NewCachedVendorWindowStore(MemoryVendorWindowStore inner, int ttl, IClock clock)
    {
        return new CachedVendorWindowStore(inner, VendorWindowCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VendorWindow> FindVendorWindows(IVendorWindowStore store, VendorWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
