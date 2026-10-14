namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop mappings by code.</summary>
public interface ILaptopMappingStore
{
    LaptopMapping? Find(string code);
    void Save(LaptopMapping item);
    bool Remove(string code);
    List<LaptopMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopMappingStore(Dictionary<string, LaptopMapping> items, List<string> order) : ILaptopMappingStore
{
    public Dictionary<string, LaptopMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopMapping item)
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

    public List<LaptopMapping> List()
    {
        var outValue = new List<LaptopMapping>();
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
public sealed class CachedLaptopMappingStore(MemoryLaptopMappingStore inner, LaptopMappingCache cache, int hits, int misses) : ILaptopMappingStore
{
    public MemoryLaptopMappingStore Inner { get; set; } = inner;
    public LaptopMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopMapping? Find(string code)
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

    public void Save(LaptopMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop mappings. Empty fields match all.</summary>
public sealed record LaptopMappingQuery(string Department, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopMapping item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopMapping> Apply(List<LaptopMapping> items)
    {
        var matched = Collections.Filter(items, (LaptopMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop mapping store.</summary>
public static class LaptopMappingStore
{
    public static ILaptopMappingStore NewMemoryLaptopMappingStore()
    {
        return new MemoryLaptopMappingStore(new Dictionary<string, LaptopMapping>(), new List<string>());
    }

    public static ILaptopMappingStore NewCachedLaptopMappingStore(MemoryLaptopMappingStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopMappingStore(inner, LaptopMappingCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LaptopMapping> FindLaptopMappings(ILaptopMappingStore store, LaptopMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
