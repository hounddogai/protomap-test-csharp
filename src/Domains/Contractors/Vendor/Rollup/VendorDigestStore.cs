namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor digests by code.</summary>
public interface IVendorDigestStore
{
    VendorDigest? Find(string code);
    void Save(VendorDigest item);
    bool Remove(string code);
    List<VendorDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorDigestStore(Dictionary<string, VendorDigest> items, List<string> order) : IVendorDigestStore
{
    public Dictionary<string, VendorDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorDigest item)
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

    public List<VendorDigest> List()
    {
        var outValue = new List<VendorDigest>();
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
public sealed class CachedVendorDigestStore(MemoryVendorDigestStore inner, VendorDigestCache cache, int hits, int misses) : IVendorDigestStore
{
    public MemoryVendorDigestStore Inner { get; set; } = inner;
    public VendorDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorDigest? Find(string code)
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

    public void Save(VendorDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor digests. Empty fields match all.</summary>
public sealed record VendorDigestQuery(string Department, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(VendorDigest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorDigest> Apply(List<VendorDigest> items)
    {
        var matched = Collections.Filter(items, (VendorDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor digest store.</summary>
public static class VendorDigestStore
{
    public static IVendorDigestStore NewMemoryVendorDigestStore()
    {
        return new MemoryVendorDigestStore(new Dictionary<string, VendorDigest>(), new List<string>());
    }

    public static IVendorDigestStore NewCachedVendorDigestStore(MemoryVendorDigestStore inner, int ttl, IClock clock)
    {
        return new CachedVendorDigestStore(inner, VendorDigestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<VendorDigest> FindVendorDigests(IVendorDigestStore store, VendorDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
