namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool metrics by code.</summary>
public interface IPoolMetricStore
{
    PoolMetric? Find(string code);
    void Save(PoolMetric item);
    bool Remove(string code);
    List<PoolMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolMetricStore(Dictionary<string, PoolMetric> items, List<string> order) : IPoolMetricStore
{
    public Dictionary<string, PoolMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolMetric item)
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

    public List<PoolMetric> List()
    {
        var outValue = new List<PoolMetric>();
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
public sealed class CachedPoolMetricStore(MemoryPoolMetricStore inner, PoolMetricCache cache, int hits, int misses) : IPoolMetricStore
{
    public MemoryPoolMetricStore Inner { get; set; } = inner;
    public PoolMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolMetric? Find(string code)
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

    public void Save(PoolMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool metrics. Empty fields match all.</summary>
public sealed record PoolMetricQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PoolMetric item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolMetric> Apply(List<PoolMetric> items)
    {
        var matched = Collections.Filter(items, (PoolMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool metric store.</summary>
public static class PoolMetricStore
{
    public static IPoolMetricStore NewMemoryPoolMetricStore()
    {
        return new MemoryPoolMetricStore(new Dictionary<string, PoolMetric>(), new List<string>());
    }

    public static IPoolMetricStore NewCachedPoolMetricStore(MemoryPoolMetricStore inner, int ttl, IClock clock)
    {
        return new CachedPoolMetricStore(inner, PoolMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PoolMetric> FindPoolMetrics(IPoolMetricStore store, PoolMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
