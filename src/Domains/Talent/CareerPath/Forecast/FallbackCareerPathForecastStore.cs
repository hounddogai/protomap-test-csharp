namespace Acme.Workforce.Talent.CareerPath.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback career path forecasts by code.</summary>
public interface IFallbackCareerPathForecastStore
{
    FallbackCareerPathForecast? Find(string code);
    void Save(FallbackCareerPathForecast item);
    bool Remove(string code);
    List<FallbackCareerPathForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackCareerPathForecastStore(Dictionary<string, FallbackCareerPathForecast> items, List<string> order) : IFallbackCareerPathForecastStore
{
    public Dictionary<string, FallbackCareerPathForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackCareerPathForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackCareerPathForecast item)
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

    public List<FallbackCareerPathForecast> List()
    {
        var outValue = new List<FallbackCareerPathForecast>();
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
public sealed class CachedFallbackCareerPathForecastStore(MemoryFallbackCareerPathForecastStore inner, FallbackCareerPathForecastCache cache, int hits, int misses) : IFallbackCareerPathForecastStore
{
    public MemoryFallbackCareerPathForecastStore Inner { get; set; } = inner;
    public FallbackCareerPathForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackCareerPathForecast? Find(string code)
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

    public void Save(FallbackCareerPathForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackCareerPathForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback career path forecasts. Empty fields match all.</summary>
public sealed record FallbackCareerPathForecastQuery(string Label, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackCareerPathForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackCareerPathForecast> Apply(List<FallbackCareerPathForecast> items)
    {
        var matched = Collections.Filter(items, (FallbackCareerPathForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback career path forecast store.</summary>
public static class FallbackCareerPathForecastStore
{
    public static IFallbackCareerPathForecastStore NewMemoryFallbackCareerPathForecastStore()
    {
        return new MemoryFallbackCareerPathForecastStore(
            new Dictionary<string, FallbackCareerPathForecast>(),
            new List<string>());
    }

    public static IFallbackCareerPathForecastStore NewCachedFallbackCareerPathForecastStore(MemoryFallbackCareerPathForecastStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackCareerPathForecastStore(
            inner,
            FallbackCareerPathForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackCareerPathForecast> FindFallbackCareerPathForecasts(IFallbackCareerPathForecastStore store, FallbackCareerPathForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
