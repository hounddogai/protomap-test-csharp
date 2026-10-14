namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default laptop digests by code.</summary>
public interface IDefaultLaptopDigestStore
{
    DefaultLaptopDigest? Find(string code);
    void Save(DefaultLaptopDigest item);
    bool Remove(string code);
    List<DefaultLaptopDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultLaptopDigestStore(Dictionary<string, DefaultLaptopDigest> items, List<string> order) : IDefaultLaptopDigestStore
{
    public Dictionary<string, DefaultLaptopDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultLaptopDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultLaptopDigest item)
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

    public List<DefaultLaptopDigest> List()
    {
        var outValue = new List<DefaultLaptopDigest>();
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
public sealed class CachedDefaultLaptopDigestStore(MemoryDefaultLaptopDigestStore inner, DefaultLaptopDigestCache cache, int hits, int misses) : IDefaultLaptopDigestStore
{
    public MemoryDefaultLaptopDigestStore Inner { get; set; } = inner;
    public DefaultLaptopDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultLaptopDigest? Find(string code)
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

    public void Save(DefaultLaptopDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultLaptopDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default laptop digests. Empty fields match all.</summary>
public sealed record DefaultLaptopDigestQuery(string Department, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultLaptopDigest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultLaptopDigest> Apply(List<DefaultLaptopDigest> items)
    {
        var matched = Collections.Filter(items, (DefaultLaptopDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default laptop digest store.</summary>
public static class DefaultLaptopDigestStore
{
    public static IDefaultLaptopDigestStore NewMemoryDefaultLaptopDigestStore()
    {
        return new MemoryDefaultLaptopDigestStore(new Dictionary<string, DefaultLaptopDigest>(), new List<string>());
    }

    public static IDefaultLaptopDigestStore NewCachedDefaultLaptopDigestStore(MemoryDefaultLaptopDigestStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultLaptopDigestStore(inner, DefaultLaptopDigestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DefaultLaptopDigest> FindDefaultLaptopDigests(IDefaultLaptopDigestStore store, DefaultLaptopDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
