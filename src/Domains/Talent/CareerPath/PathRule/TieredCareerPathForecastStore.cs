namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered career path forecasts by code.</summary>
public interface ITieredCareerPathForecastStore
{
    TieredCareerPathForecast? Find(string code);
    void Save(TieredCareerPathForecast item);
    bool Remove(string code);
    List<TieredCareerPathForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredCareerPathForecastStore(Dictionary<string, TieredCareerPathForecast> items, List<string> order) : ITieredCareerPathForecastStore
{
    public Dictionary<string, TieredCareerPathForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredCareerPathForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredCareerPathForecast item)
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

    public List<TieredCareerPathForecast> List()
    {
        var outValue = new List<TieredCareerPathForecast>();
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
public sealed class CachedTieredCareerPathForecastStore(MemoryTieredCareerPathForecastStore inner, TieredCareerPathForecastCache cache, int hits, int misses) : ITieredCareerPathForecastStore
{
    public MemoryTieredCareerPathForecastStore Inner { get; set; } = inner;
    public TieredCareerPathForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredCareerPathForecast? Find(string code)
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

    public void Save(TieredCareerPathForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredCareerPathForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered career path forecasts. Empty fields match all.</summary>
public sealed record TieredCareerPathForecastQuery(string Region, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(TieredCareerPathForecast item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredCareerPathForecast> Apply(List<TieredCareerPathForecast> items)
    {
        var matched = Collections.Filter(items, (TieredCareerPathForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered career path forecast store.</summary>
public static class TieredCareerPathForecastStore
{
    public static ITieredCareerPathForecastStore NewMemoryTieredCareerPathForecastStore()
    {
        return new MemoryTieredCareerPathForecastStore(
            new Dictionary<string, TieredCareerPathForecast>(),
            new List<string>());
    }

    public static ITieredCareerPathForecastStore NewCachedTieredCareerPathForecastStore(MemoryTieredCareerPathForecastStore inner, int ttl, IClock clock)
    {
        return new CachedTieredCareerPathForecastStore(
            inner,
            TieredCareerPathForecastCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<TieredCareerPathForecast> FindTieredCareerPathForecasts(ITieredCareerPathForecastStore store, TieredCareerPathForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
