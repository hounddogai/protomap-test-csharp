namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped laptop rollups by code.</summary>
public interface ICappedLaptopRollupStore
{
    CappedLaptopRollup? Find(string code);
    void Save(CappedLaptopRollup item);
    bool Remove(string code);
    List<CappedLaptopRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedLaptopRollupStore(Dictionary<string, CappedLaptopRollup> items, List<string> order) : ICappedLaptopRollupStore
{
    public Dictionary<string, CappedLaptopRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedLaptopRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedLaptopRollup item)
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

    public List<CappedLaptopRollup> List()
    {
        var outValue = new List<CappedLaptopRollup>();
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
public sealed class CachedCappedLaptopRollupStore(MemoryCappedLaptopRollupStore inner, CappedLaptopRollupCache cache, int hits, int misses) : ICappedLaptopRollupStore
{
    public MemoryCappedLaptopRollupStore Inner { get; set; } = inner;
    public CappedLaptopRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedLaptopRollup? Find(string code)
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

    public void Save(CappedLaptopRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedLaptopRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped laptop rollups. Empty fields match all.</summary>
public sealed record CappedLaptopRollupQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(CappedLaptopRollup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<CappedLaptopRollup> Apply(List<CappedLaptopRollup> items)
    {
        var matched = Collections.Filter(items, (CappedLaptopRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped laptop rollup store.</summary>
public static class CappedLaptopRollupStore
{
    public static ICappedLaptopRollupStore NewMemoryCappedLaptopRollupStore()
    {
        return new MemoryCappedLaptopRollupStore(new Dictionary<string, CappedLaptopRollup>(), new List<string>());
    }

    public static ICappedLaptopRollupStore NewCachedCappedLaptopRollupStore(MemoryCappedLaptopRollupStore inner, int ttl, IClock clock)
    {
        return new CachedCappedLaptopRollupStore(inner, CappedLaptopRollupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CappedLaptopRollup> FindCappedLaptopRollups(ICappedLaptopRollupStore store, CappedLaptopRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
