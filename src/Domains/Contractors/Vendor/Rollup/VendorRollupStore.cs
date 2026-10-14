namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor rollups by code.</summary>
public interface IVendorRollupStore
{
    VendorRollup? Find(string code);
    void Save(VendorRollup item);
    bool Remove(string code);
    List<VendorRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorRollupStore(Dictionary<string, VendorRollup> items, List<string> order) : IVendorRollupStore
{
    public Dictionary<string, VendorRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorRollup item)
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

    public List<VendorRollup> List()
    {
        var outValue = new List<VendorRollup>();
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
public sealed class CachedVendorRollupStore(MemoryVendorRollupStore inner, VendorRollupCache cache, int hits, int misses) : IVendorRollupStore
{
    public MemoryVendorRollupStore Inner { get; set; } = inner;
    public VendorRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorRollup? Find(string code)
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

    public void Save(VendorRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor rollups. Empty fields match all.</summary>
public sealed record VendorRollupQuery(string Department, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(VendorRollup item)
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

    public List<VendorRollup> Apply(List<VendorRollup> items)
    {
        var matched = Collections.Filter(items, (VendorRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor rollup store.</summary>
public static class VendorRollupStore
{
    public static IVendorRollupStore NewMemoryVendorRollupStore()
    {
        return new MemoryVendorRollupStore(new Dictionary<string, VendorRollup>(), new List<string>());
    }

    public static IVendorRollupStore NewCachedVendorRollupStore(MemoryVendorRollupStore inner, int ttl, IClock clock)
    {
        return new CachedVendorRollupStore(inner, VendorRollupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VendorRollup> FindVendorRollups(IVendorRollupStore store, VendorRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
