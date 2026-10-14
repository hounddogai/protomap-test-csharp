namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool tiers by code.</summary>
public interface IPoolTierStore
{
    PoolTier? Find(string code);
    void Save(PoolTier item);
    bool Remove(string code);
    List<PoolTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolTierStore(Dictionary<string, PoolTier> items, List<string> order) : IPoolTierStore
{
    public Dictionary<string, PoolTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolTier item)
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

    public List<PoolTier> List()
    {
        var outValue = new List<PoolTier>();
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
public sealed class CachedPoolTierStore(MemoryPoolTierStore inner, PoolTierCache cache, int hits, int misses) : IPoolTierStore
{
    public MemoryPoolTierStore Inner { get; set; } = inner;
    public PoolTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolTier? Find(string code)
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

    public void Save(PoolTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool tiers. Empty fields match all.</summary>
public sealed record PoolTierQuery(string Category, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PoolTier item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolTier> Apply(List<PoolTier> items)
    {
        var matched = Collections.Filter(items, (PoolTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool tier store.</summary>
public static class PoolTierStore
{
    public static IPoolTierStore NewMemoryPoolTierStore()
    {
        return new MemoryPoolTierStore(new Dictionary<string, PoolTier>(), new List<string>());
    }

    public static IPoolTierStore NewCachedPoolTierStore(MemoryPoolTierStore inner, int ttl, IClock clock)
    {
        return new CachedPoolTierStore(inner, PoolTierCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PoolTier> FindPoolTiers(IPoolTierStore store, PoolTierQuery query)
    {
        return query.Apply(store.List());
    }
}
