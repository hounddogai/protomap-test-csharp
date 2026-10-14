namespace Acme.Workforce.Contractors.Vendor.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict vendor requests by code.</summary>
public interface IStrictVendorRequestStore
{
    StrictVendorRequest? Find(string code);
    void Save(StrictVendorRequest item);
    bool Remove(string code);
    List<StrictVendorRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictVendorRequestStore(Dictionary<string, StrictVendorRequest> items, List<string> order) : IStrictVendorRequestStore
{
    public Dictionary<string, StrictVendorRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictVendorRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictVendorRequest item)
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

    public List<StrictVendorRequest> List()
    {
        var outValue = new List<StrictVendorRequest>();
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
public sealed class CachedStrictVendorRequestStore(MemoryStrictVendorRequestStore inner, StrictVendorRequestCache cache, int hits, int misses) : IStrictVendorRequestStore
{
    public MemoryStrictVendorRequestStore Inner { get; set; } = inner;
    public StrictVendorRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictVendorRequest? Find(string code)
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

    public void Save(StrictVendorRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictVendorRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict vendor requests. Empty fields match all.</summary>
public sealed record StrictVendorRequestQuery(string Title, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictVendorRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<StrictVendorRequest> Apply(List<StrictVendorRequest> items)
    {
        var matched = Collections.Filter(items, (StrictVendorRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict vendor request store.</summary>
public static class StrictVendorRequestStore
{
    public static IStrictVendorRequestStore NewMemoryStrictVendorRequestStore()
    {
        return new MemoryStrictVendorRequestStore(new Dictionary<string, StrictVendorRequest>(), new List<string>());
    }

    public static IStrictVendorRequestStore NewCachedStrictVendorRequestStore(MemoryStrictVendorRequestStore inner, int ttl, IClock clock)
    {
        return new CachedStrictVendorRequestStore(inner, StrictVendorRequestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StrictVendorRequest> FindStrictVendorRequests(IStrictVendorRequestStore store, StrictVendorRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
