namespace Acme.Workforce.Assets.Laptop.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop groups by code.</summary>
public interface ILaptopGroupStore
{
    LaptopGroup? Find(string code);
    void Save(LaptopGroup item);
    bool Remove(string code);
    List<LaptopGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopGroupStore(Dictionary<string, LaptopGroup> items, List<string> order) : ILaptopGroupStore
{
    public Dictionary<string, LaptopGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopGroup item)
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

    public List<LaptopGroup> List()
    {
        var outValue = new List<LaptopGroup>();
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
public sealed class CachedLaptopGroupStore(MemoryLaptopGroupStore inner, LaptopGroupCache cache, int hits, int misses) : ILaptopGroupStore
{
    public MemoryLaptopGroupStore Inner { get; set; } = inner;
    public LaptopGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopGroup? Find(string code)
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

    public void Save(LaptopGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop groups. Empty fields match all.</summary>
public sealed record LaptopGroupQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopGroup> Apply(List<LaptopGroup> items)
    {
        var matched = Collections.Filter(items, (LaptopGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop group store.</summary>
public static class LaptopGroupStore
{
    public static ILaptopGroupStore NewMemoryLaptopGroupStore()
    {
        return new MemoryLaptopGroupStore(new Dictionary<string, LaptopGroup>(), new List<string>());
    }

    public static ILaptopGroupStore NewCachedLaptopGroupStore(MemoryLaptopGroupStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopGroupStore(inner, LaptopGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LaptopGroup> FindLaptopGroups(ILaptopGroupStore store, LaptopGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
