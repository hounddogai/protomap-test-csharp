namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor policies by code.</summary>
public interface IVendorPolicyStore
{
    VendorPolicy? Find(string code);
    void Save(VendorPolicy item);
    bool Remove(string code);
    List<VendorPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorPolicyStore(Dictionary<string, VendorPolicy> items, List<string> order) : IVendorPolicyStore
{
    public Dictionary<string, VendorPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorPolicy item)
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

    public List<VendorPolicy> List()
    {
        var outValue = new List<VendorPolicy>();
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
public sealed class CachedVendorPolicyStore(MemoryVendorPolicyStore inner, VendorPolicyCache cache, int hits, int misses) : IVendorPolicyStore
{
    public MemoryVendorPolicyStore Inner { get; set; } = inner;
    public VendorPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorPolicy? Find(string code)
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

    public void Save(VendorPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor policies. Empty fields match all.</summary>
public sealed record VendorPolicyQuery(string Country, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(VendorPolicy item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorPolicy> Apply(List<VendorPolicy> items)
    {
        var matched = Collections.Filter(items, (VendorPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor policy store.</summary>
public static class VendorPolicyStore
{
    public static IVendorPolicyStore NewMemoryVendorPolicyStore()
    {
        return new MemoryVendorPolicyStore(new Dictionary<string, VendorPolicy>(), new List<string>());
    }

    public static IVendorPolicyStore NewCachedVendorPolicyStore(MemoryVendorPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedVendorPolicyStore(inner, VendorPolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VendorPolicy> FindVendorPolicies(IVendorPolicyStore store, VendorPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
