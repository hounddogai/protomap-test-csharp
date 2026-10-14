namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback asset return forecasts by code.</summary>
public interface IFallbackAssetReturnForecastStore
{
    FallbackAssetReturnForecast? Find(string code);
    void Save(FallbackAssetReturnForecast item);
    bool Remove(string code);
    List<FallbackAssetReturnForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackAssetReturnForecastStore(Dictionary<string, FallbackAssetReturnForecast> items, List<string> order) : IFallbackAssetReturnForecastStore
{
    public Dictionary<string, FallbackAssetReturnForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackAssetReturnForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackAssetReturnForecast item)
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

    public List<FallbackAssetReturnForecast> List()
    {
        var outValue = new List<FallbackAssetReturnForecast>();
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
public sealed class CachedFallbackAssetReturnForecastStore(MemoryFallbackAssetReturnForecastStore inner, FallbackAssetReturnForecastCache cache, int hits, int misses) : IFallbackAssetReturnForecastStore
{
    public MemoryFallbackAssetReturnForecastStore Inner { get; set; } = inner;
    public FallbackAssetReturnForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackAssetReturnForecast? Find(string code)
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

    public void Save(FallbackAssetReturnForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackAssetReturnForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback asset return forecasts. Empty fields match all.</summary>
public sealed record FallbackAssetReturnForecastQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackAssetReturnForecast item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<FallbackAssetReturnForecast> Apply(List<FallbackAssetReturnForecast> items)
    {
        var matched = Collections.Filter(items, (FallbackAssetReturnForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback asset return forecast store.</summary>
public static class FallbackAssetReturnForecastStore
{
    public static IFallbackAssetReturnForecastStore NewMemoryFallbackAssetReturnForecastStore()
    {
        return new MemoryFallbackAssetReturnForecastStore(
            new Dictionary<string, FallbackAssetReturnForecast>(),
            new List<string>());
    }

    public static IFallbackAssetReturnForecastStore NewCachedFallbackAssetReturnForecastStore(MemoryFallbackAssetReturnForecastStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackAssetReturnForecastStore(
            inner,
            FallbackAssetReturnForecastCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<FallbackAssetReturnForecast> FindFallbackAssetReturnForecasts(IFallbackAssetReturnForecastStore store, FallbackAssetReturnForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
