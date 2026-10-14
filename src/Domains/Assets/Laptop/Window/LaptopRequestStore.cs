namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop requests by code.</summary>
public interface ILaptopRequestStore
{
    LaptopRequest? Find(string code);
    void Save(LaptopRequest item);
    bool Remove(string code);
    List<LaptopRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopRequestStore(Dictionary<string, LaptopRequest> items, List<string> order) : ILaptopRequestStore
{
    public Dictionary<string, LaptopRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopRequest item)
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

    public List<LaptopRequest> List()
    {
        var outValue = new List<LaptopRequest>();
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
public sealed class CachedLaptopRequestStore(MemoryLaptopRequestStore inner, LaptopRequestCache cache, int hits, int misses) : ILaptopRequestStore
{
    public MemoryLaptopRequestStore Inner { get; set; } = inner;
    public LaptopRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopRequest? Find(string code)
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

    public void Save(LaptopRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop requests. Empty fields match all.</summary>
public sealed record LaptopRequestQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopRequest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopRequest> Apply(List<LaptopRequest> items)
    {
        var matched = Collections.Filter(items, (LaptopRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop request store.</summary>
public static class LaptopRequestStore
{
    public static ILaptopRequestStore NewMemoryLaptopRequestStore()
    {
        return new MemoryLaptopRequestStore(new Dictionary<string, LaptopRequest>(), new List<string>());
    }

    public static ILaptopRequestStore NewCachedLaptopRequestStore(MemoryLaptopRequestStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopRequestStore(inner, LaptopRequestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LaptopRequest> FindLaptopRequests(ILaptopRequestStore store, LaptopRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
