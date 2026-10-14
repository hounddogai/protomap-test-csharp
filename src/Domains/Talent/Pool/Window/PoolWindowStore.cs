namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool windows by code.</summary>
public interface IPoolWindowStore
{
    PoolWindow? Find(string code);
    void Save(PoolWindow item);
    bool Remove(string code);
    List<PoolWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolWindowStore(Dictionary<string, PoolWindow> items, List<string> order) : IPoolWindowStore
{
    public Dictionary<string, PoolWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolWindow item)
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

    public List<PoolWindow> List()
    {
        var outValue = new List<PoolWindow>();
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
public sealed class CachedPoolWindowStore(MemoryPoolWindowStore inner, PoolWindowCache cache, int hits, int misses) : IPoolWindowStore
{
    public MemoryPoolWindowStore Inner { get; set; } = inner;
    public PoolWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolWindow? Find(string code)
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

    public void Save(PoolWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool windows. Empty fields match all.</summary>
public sealed record PoolWindowQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(PoolWindow item)
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

    public List<PoolWindow> Apply(List<PoolWindow> items)
    {
        var matched = Collections.Filter(items, (PoolWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool window store.</summary>
public static class PoolWindowStore
{
    public static IPoolWindowStore NewMemoryPoolWindowStore()
    {
        return new MemoryPoolWindowStore(new Dictionary<string, PoolWindow>(), new List<string>());
    }

    public static IPoolWindowStore NewCachedPoolWindowStore(MemoryPoolWindowStore inner, int ttl, IClock clock)
    {
        return new CachedPoolWindowStore(inner, PoolWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PoolWindow> FindPoolWindows(IPoolWindowStore store, PoolWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
