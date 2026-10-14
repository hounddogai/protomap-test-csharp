namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop profiles by code.</summary>
public interface ILaptopProfileStore
{
    LaptopProfile? Find(string code);
    void Save(LaptopProfile item);
    bool Remove(string code);
    List<LaptopProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopProfileStore(Dictionary<string, LaptopProfile> items, List<string> order) : ILaptopProfileStore
{
    public Dictionary<string, LaptopProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopProfile item)
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

    public List<LaptopProfile> List()
    {
        var outValue = new List<LaptopProfile>();
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
public sealed class CachedLaptopProfileStore(MemoryLaptopProfileStore inner, LaptopProfileCache cache, int hits, int misses) : ILaptopProfileStore
{
    public MemoryLaptopProfileStore Inner { get; set; } = inner;
    public LaptopProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopProfile? Find(string code)
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

    public void Save(LaptopProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop profiles. Empty fields match all.</summary>
public sealed record LaptopProfileQuery(string Channel, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopProfile item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopProfile> Apply(List<LaptopProfile> items)
    {
        var matched = Collections.Filter(items, (LaptopProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop profile store.</summary>
public static class LaptopProfileStore
{
    public static ILaptopProfileStore NewMemoryLaptopProfileStore()
    {
        return new MemoryLaptopProfileStore(new Dictionary<string, LaptopProfile>(), new List<string>());
    }

    public static ILaptopProfileStore NewCachedLaptopProfileStore(MemoryLaptopProfileStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopProfileStore(inner, LaptopProfileCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LaptopProfile> FindLaptopProfiles(ILaptopProfileStore store, LaptopProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
