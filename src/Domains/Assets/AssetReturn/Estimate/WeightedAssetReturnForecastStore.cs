namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted asset return forecasts by code.</summary>
public interface IWeightedAssetReturnForecastStore
{
    WeightedAssetReturnForecast? Find(string code);
    void Save(WeightedAssetReturnForecast item);
    bool Remove(string code);
    List<WeightedAssetReturnForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAssetReturnForecastStore(Dictionary<string, WeightedAssetReturnForecast> items, List<string> order) : IWeightedAssetReturnForecastStore
{
    public Dictionary<string, WeightedAssetReturnForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAssetReturnForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAssetReturnForecast item)
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

    public List<WeightedAssetReturnForecast> List()
    {
        var outValue = new List<WeightedAssetReturnForecast>();
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
public sealed class CachedWeightedAssetReturnForecastStore(MemoryWeightedAssetReturnForecastStore inner, WeightedAssetReturnForecastCache cache, int hits, int misses) : IWeightedAssetReturnForecastStore
{
    public MemoryWeightedAssetReturnForecastStore Inner { get; set; } = inner;
    public WeightedAssetReturnForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAssetReturnForecast? Find(string code)
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

    public void Save(WeightedAssetReturnForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAssetReturnForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted asset return forecasts. Empty fields match all.</summary>
public sealed record WeightedAssetReturnForecastQuery(string Country, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAssetReturnForecast item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAssetReturnForecast> Apply(List<WeightedAssetReturnForecast> items)
    {
        var matched = Collections.Filter(items, (WeightedAssetReturnForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted asset return forecast store.</summary>
public static class WeightedAssetReturnForecastStore
{
    public static IWeightedAssetReturnForecastStore NewMemoryWeightedAssetReturnForecastStore()
    {
        return new MemoryWeightedAssetReturnForecastStore(
            new Dictionary<string, WeightedAssetReturnForecast>(),
            new List<string>());
    }

    public static IWeightedAssetReturnForecastStore NewCachedWeightedAssetReturnForecastStore(MemoryWeightedAssetReturnForecastStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAssetReturnForecastStore(
            inner,
            WeightedAssetReturnForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAssetReturnForecast> FindWeightedAssetReturnForecasts(IWeightedAssetReturnForecastStore store, WeightedAssetReturnForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
