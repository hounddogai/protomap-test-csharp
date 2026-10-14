namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered laptop windows by code.</summary>
public interface ITieredLaptopWindowStore
{
    TieredLaptopWindow? Find(string code);
    void Save(TieredLaptopWindow item);
    bool Remove(string code);
    List<TieredLaptopWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredLaptopWindowStore(Dictionary<string, TieredLaptopWindow> items, List<string> order) : ITieredLaptopWindowStore
{
    public Dictionary<string, TieredLaptopWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredLaptopWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredLaptopWindow item)
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

    public List<TieredLaptopWindow> List()
    {
        var outValue = new List<TieredLaptopWindow>();
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
public sealed class CachedTieredLaptopWindowStore(MemoryTieredLaptopWindowStore inner, TieredLaptopWindowCache cache, int hits, int misses) : ITieredLaptopWindowStore
{
    public MemoryTieredLaptopWindowStore Inner { get; set; } = inner;
    public TieredLaptopWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredLaptopWindow? Find(string code)
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

    public void Save(TieredLaptopWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredLaptopWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered laptop windows. Empty fields match all.</summary>
public sealed record TieredLaptopWindowQuery(string Unit, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(TieredLaptopWindow item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredLaptopWindow> Apply(List<TieredLaptopWindow> items)
    {
        var matched = Collections.Filter(items, (TieredLaptopWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered laptop window store.</summary>
public static class TieredLaptopWindowStore
{
    public static ITieredLaptopWindowStore NewMemoryTieredLaptopWindowStore()
    {
        return new MemoryTieredLaptopWindowStore(new Dictionary<string, TieredLaptopWindow>(), new List<string>());
    }

    public static ITieredLaptopWindowStore NewCachedTieredLaptopWindowStore(MemoryTieredLaptopWindowStore inner, int ttl, IClock clock)
    {
        return new CachedTieredLaptopWindowStore(inner, TieredLaptopWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TieredLaptopWindow> FindTieredLaptopWindows(ITieredLaptopWindowStore store, TieredLaptopWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
