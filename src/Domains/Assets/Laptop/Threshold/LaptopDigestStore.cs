namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop digests by code.</summary>
public interface ILaptopDigestStore
{
    LaptopDigest? Find(string code);
    void Save(LaptopDigest item);
    bool Remove(string code);
    List<LaptopDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopDigestStore(Dictionary<string, LaptopDigest> items, List<string> order) : ILaptopDigestStore
{
    public Dictionary<string, LaptopDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopDigest item)
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

    public List<LaptopDigest> List()
    {
        var outValue = new List<LaptopDigest>();
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
public sealed class CachedLaptopDigestStore(MemoryLaptopDigestStore inner, LaptopDigestCache cache, int hits, int misses) : ILaptopDigestStore
{
    public MemoryLaptopDigestStore Inner { get; set; } = inner;
    public LaptopDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopDigest? Find(string code)
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

    public void Save(LaptopDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop digests. Empty fields match all.</summary>
public sealed record LaptopDigestQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopDigest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopDigest> Apply(List<LaptopDigest> items)
    {
        var matched = Collections.Filter(items, (LaptopDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop digest store.</summary>
public static class LaptopDigestStore
{
    public static ILaptopDigestStore NewMemoryLaptopDigestStore()
    {
        return new MemoryLaptopDigestStore(new Dictionary<string, LaptopDigest>(), new List<string>());
    }

    public static ILaptopDigestStore NewCachedLaptopDigestStore(MemoryLaptopDigestStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopDigestStore(inner, LaptopDigestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LaptopDigest> FindLaptopDigests(ILaptopDigestStore store, LaptopDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
