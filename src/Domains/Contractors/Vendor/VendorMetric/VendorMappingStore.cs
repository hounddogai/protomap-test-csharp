namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor mappings by code.</summary>
public interface IVendorMappingStore
{
    VendorMapping? Find(string code);
    void Save(VendorMapping item);
    bool Remove(string code);
    List<VendorMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorMappingStore(Dictionary<string, VendorMapping> items, List<string> order) : IVendorMappingStore
{
    public Dictionary<string, VendorMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorMapping item)
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

    public List<VendorMapping> List()
    {
        var outValue = new List<VendorMapping>();
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
public sealed class CachedVendorMappingStore(MemoryVendorMappingStore inner, VendorMappingCache cache, int hits, int misses) : IVendorMappingStore
{
    public MemoryVendorMappingStore Inner { get; set; } = inner;
    public VendorMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorMapping? Find(string code)
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

    public void Save(VendorMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor mappings. Empty fields match all.</summary>
public sealed record VendorMappingQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(VendorMapping item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorMapping> Apply(List<VendorMapping> items)
    {
        var matched = Collections.Filter(items, (VendorMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor mapping store.</summary>
public static class VendorMappingStore
{
    public static IVendorMappingStore NewMemoryVendorMappingStore()
    {
        return new MemoryVendorMappingStore(new Dictionary<string, VendorMapping>(), new List<string>());
    }

    public static IVendorMappingStore NewCachedVendorMappingStore(MemoryVendorMappingStore inner, int ttl, IClock clock)
    {
        return new CachedVendorMappingStore(inner, VendorMappingCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VendorMapping> FindVendorMappings(IVendorMappingStore store, VendorMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
