namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool variances by code.</summary>
public interface IPoolVarianceStore
{
    PoolVariance? Find(string code);
    void Save(PoolVariance item);
    bool Remove(string code);
    List<PoolVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolVarianceStore(Dictionary<string, PoolVariance> items, List<string> order) : IPoolVarianceStore
{
    public Dictionary<string, PoolVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolVariance item)
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

    public List<PoolVariance> List()
    {
        var outValue = new List<PoolVariance>();
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
public sealed class CachedPoolVarianceStore(MemoryPoolVarianceStore inner, PoolVarianceCache cache, int hits, int misses) : IPoolVarianceStore
{
    public MemoryPoolVarianceStore Inner { get; set; } = inner;
    public PoolVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolVariance? Find(string code)
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

    public void Save(PoolVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool variances. Empty fields match all.</summary>
public sealed record PoolVarianceQuery(string Locale, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PoolVariance item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolVariance> Apply(List<PoolVariance> items)
    {
        var matched = Collections.Filter(items, (PoolVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool variance store.</summary>
public static class PoolVarianceStore
{
    public static IPoolVarianceStore NewMemoryPoolVarianceStore()
    {
        return new MemoryPoolVarianceStore(new Dictionary<string, PoolVariance>(), new List<string>());
    }

    public static IPoolVarianceStore NewCachedPoolVarianceStore(MemoryPoolVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedPoolVarianceStore(inner, PoolVarianceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PoolVariance> FindPoolVariances(IPoolVarianceStore store, PoolVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
