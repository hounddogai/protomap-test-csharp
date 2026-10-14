namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient laptop windows by code.</summary>
public interface ILenientLaptopWindowStore
{
    LenientLaptopWindow? Find(string code);
    void Save(LenientLaptopWindow item);
    bool Remove(string code);
    List<LenientLaptopWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientLaptopWindowStore(Dictionary<string, LenientLaptopWindow> items, List<string> order) : ILenientLaptopWindowStore
{
    public Dictionary<string, LenientLaptopWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientLaptopWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientLaptopWindow item)
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

    public List<LenientLaptopWindow> List()
    {
        var outValue = new List<LenientLaptopWindow>();
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
public sealed class CachedLenientLaptopWindowStore(MemoryLenientLaptopWindowStore inner, LenientLaptopWindowCache cache, int hits, int misses) : ILenientLaptopWindowStore
{
    public MemoryLenientLaptopWindowStore Inner { get; set; } = inner;
    public LenientLaptopWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientLaptopWindow? Find(string code)
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

    public void Save(LenientLaptopWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientLaptopWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient laptop windows. Empty fields match all.</summary>
public sealed record LenientLaptopWindowQuery(string Department, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(LenientLaptopWindow item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<LenientLaptopWindow> Apply(List<LenientLaptopWindow> items)
    {
        var matched = Collections.Filter(items, (LenientLaptopWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient laptop window store.</summary>
public static class LenientLaptopWindowStore
{
    public static ILenientLaptopWindowStore NewMemoryLenientLaptopWindowStore()
    {
        return new MemoryLenientLaptopWindowStore(new Dictionary<string, LenientLaptopWindow>(), new List<string>());
    }

    public static ILenientLaptopWindowStore NewCachedLenientLaptopWindowStore(MemoryLenientLaptopWindowStore inner, int ttl, IClock clock)
    {
        return new CachedLenientLaptopWindowStore(inner, LenientLaptopWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LenientLaptopWindow> FindLenientLaptopWindows(ILenientLaptopWindowStore store, LenientLaptopWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
