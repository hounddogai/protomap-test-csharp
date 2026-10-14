namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool forecasts by code.</summary>
public interface IPoolForecastStore
{
    PoolForecast? Find(string code);
    void Save(PoolForecast item);
    bool Remove(string code);
    List<PoolForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolForecastStore(Dictionary<string, PoolForecast> items, List<string> order) : IPoolForecastStore
{
    public Dictionary<string, PoolForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolForecast item)
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

    public List<PoolForecast> List()
    {
        var outValue = new List<PoolForecast>();
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
public sealed class CachedPoolForecastStore(MemoryPoolForecastStore inner, PoolForecastCache cache, int hits, int misses) : IPoolForecastStore
{
    public MemoryPoolForecastStore Inner { get; set; } = inner;
    public PoolForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolForecast? Find(string code)
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

    public void Save(PoolForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool forecasts. Empty fields match all.</summary>
public sealed record PoolForecastQuery(string Category, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(PoolForecast item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<PoolForecast> Apply(List<PoolForecast> items)
    {
        var matched = Collections.Filter(items, (PoolForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool forecast store.</summary>
public static class PoolForecastStore
{
    public static IPoolForecastStore NewMemoryPoolForecastStore()
    {
        return new MemoryPoolForecastStore(new Dictionary<string, PoolForecast>(), new List<string>());
    }

    public static IPoolForecastStore NewCachedPoolForecastStore(MemoryPoolForecastStore inner, int ttl, IClock clock)
    {
        return new CachedPoolForecastStore(inner, PoolForecastCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PoolForecast> FindPoolForecasts(IPoolForecastStore store, PoolForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
