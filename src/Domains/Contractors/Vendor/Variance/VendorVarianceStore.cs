namespace Acme.Workforce.Contractors.Vendor.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor variances by code.</summary>
public interface IVendorVarianceStore
{
    VendorVariance? Find(string code);
    void Save(VendorVariance item);
    bool Remove(string code);
    List<VendorVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorVarianceStore(Dictionary<string, VendorVariance> items, List<string> order) : IVendorVarianceStore
{
    public Dictionary<string, VendorVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorVariance item)
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

    public List<VendorVariance> List()
    {
        var outValue = new List<VendorVariance>();
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
public sealed class CachedVendorVarianceStore(MemoryVendorVarianceStore inner, VendorVarianceCache cache, int hits, int misses) : IVendorVarianceStore
{
    public MemoryVendorVarianceStore Inner { get; set; } = inner;
    public VendorVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorVariance? Find(string code)
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

    public void Save(VendorVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor variances. Empty fields match all.</summary>
public sealed record VendorVarianceQuery(string Unit, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(VendorVariance item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorVariance> Apply(List<VendorVariance> items)
    {
        var matched = Collections.Filter(items, (VendorVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor variance store.</summary>
public static class VendorVarianceStore
{
    public static IVendorVarianceStore NewMemoryVendorVarianceStore()
    {
        return new MemoryVendorVarianceStore(new Dictionary<string, VendorVariance>(), new List<string>());
    }

    public static IVendorVarianceStore NewCachedVendorVarianceStore(MemoryVendorVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedVendorVarianceStore(inner, VendorVarianceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VendorVariance> FindVendorVariances(IVendorVarianceStore store, VendorVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
