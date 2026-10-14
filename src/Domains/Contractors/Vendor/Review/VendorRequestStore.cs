namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor requests by code.</summary>
public interface IVendorRequestStore
{
    VendorRequest? Find(string code);
    void Save(VendorRequest item);
    bool Remove(string code);
    List<VendorRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorRequestStore(Dictionary<string, VendorRequest> items, List<string> order) : IVendorRequestStore
{
    public Dictionary<string, VendorRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorRequest item)
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

    public List<VendorRequest> List()
    {
        var outValue = new List<VendorRequest>();
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
public sealed class CachedVendorRequestStore(MemoryVendorRequestStore inner, VendorRequestCache cache, int hits, int misses) : IVendorRequestStore
{
    public MemoryVendorRequestStore Inner { get; set; } = inner;
    public VendorRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorRequest? Find(string code)
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

    public void Save(VendorRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor requests. Empty fields match all.</summary>
public sealed record VendorRequestQuery(string Department, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(VendorRequest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorRequest> Apply(List<VendorRequest> items)
    {
        var matched = Collections.Filter(items, (VendorRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor request store.</summary>
public static class VendorRequestStore
{
    public static IVendorRequestStore NewMemoryVendorRequestStore()
    {
        return new MemoryVendorRequestStore(new Dictionary<string, VendorRequest>(), new List<string>());
    }

    public static IVendorRequestStore NewCachedVendorRequestStore(MemoryVendorRequestStore inner, int ttl, IClock clock)
    {
        return new CachedVendorRequestStore(inner, VendorRequestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<VendorRequest> FindVendorRequests(IVendorRequestStore store, VendorRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
