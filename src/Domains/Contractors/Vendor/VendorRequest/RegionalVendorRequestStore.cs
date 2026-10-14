namespace Acme.Workforce.Contractors.Vendor.VendorRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional vendor requests by code.</summary>
public interface IRegionalVendorRequestStore
{
    RegionalVendorRequest? Find(string code);
    void Save(RegionalVendorRequest item);
    bool Remove(string code);
    List<RegionalVendorRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalVendorRequestStore(Dictionary<string, RegionalVendorRequest> items, List<string> order) : IRegionalVendorRequestStore
{
    public Dictionary<string, RegionalVendorRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalVendorRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalVendorRequest item)
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

    public List<RegionalVendorRequest> List()
    {
        var outValue = new List<RegionalVendorRequest>();
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
public sealed class CachedRegionalVendorRequestStore(MemoryRegionalVendorRequestStore inner, RegionalVendorRequestCache cache, int hits, int misses) : IRegionalVendorRequestStore
{
    public MemoryRegionalVendorRequestStore Inner { get; set; } = inner;
    public RegionalVendorRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalVendorRequest? Find(string code)
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

    public void Save(RegionalVendorRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalVendorRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional vendor requests. Empty fields match all.</summary>
public sealed record RegionalVendorRequestQuery(string Country, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalVendorRequest item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalVendorRequest> Apply(List<RegionalVendorRequest> items)
    {
        var matched = Collections.Filter(items, (RegionalVendorRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional vendor request store.</summary>
public static class RegionalVendorRequestStore
{
    public static IRegionalVendorRequestStore NewMemoryRegionalVendorRequestStore()
    {
        return new MemoryRegionalVendorRequestStore(
            new Dictionary<string, RegionalVendorRequest>(),
            new List<string>());
    }

    public static IRegionalVendorRequestStore NewCachedRegionalVendorRequestStore(MemoryRegionalVendorRequestStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalVendorRequestStore(inner, RegionalVendorRequestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RegionalVendorRequest> FindRegionalVendorRequests(IRegionalVendorRequestStore store, RegionalVendorRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
