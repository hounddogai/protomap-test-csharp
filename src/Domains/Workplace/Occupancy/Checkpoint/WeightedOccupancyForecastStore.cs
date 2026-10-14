namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted occupancy forecasts by code.</summary>
public interface IWeightedOccupancyForecastStore
{
    WeightedOccupancyForecast? Find(string code);
    void Save(WeightedOccupancyForecast item);
    bool Remove(string code);
    List<WeightedOccupancyForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedOccupancyForecastStore(Dictionary<string, WeightedOccupancyForecast> items, List<string> order) : IWeightedOccupancyForecastStore
{
    public Dictionary<string, WeightedOccupancyForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedOccupancyForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedOccupancyForecast item)
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

    public List<WeightedOccupancyForecast> List()
    {
        var outValue = new List<WeightedOccupancyForecast>();
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
public sealed class CachedWeightedOccupancyForecastStore(MemoryWeightedOccupancyForecastStore inner, WeightedOccupancyForecastCache cache, int hits, int misses) : IWeightedOccupancyForecastStore
{
    public MemoryWeightedOccupancyForecastStore Inner { get; set; } = inner;
    public WeightedOccupancyForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedOccupancyForecast? Find(string code)
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

    public void Save(WeightedOccupancyForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedOccupancyForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted occupancy forecasts. Empty fields match all.</summary>
public sealed record WeightedOccupancyForecastQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedOccupancyForecast item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedOccupancyForecast> Apply(List<WeightedOccupancyForecast> items)
    {
        var matched = Collections.Filter(items, (WeightedOccupancyForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted occupancy forecast store.</summary>
public static class WeightedOccupancyForecastStore
{
    public static IWeightedOccupancyForecastStore NewMemoryWeightedOccupancyForecastStore()
    {
        return new MemoryWeightedOccupancyForecastStore(
            new Dictionary<string, WeightedOccupancyForecast>(),
            new List<string>());
    }

    public static IWeightedOccupancyForecastStore NewCachedWeightedOccupancyForecastStore(MemoryWeightedOccupancyForecastStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedOccupancyForecastStore(
            inner,
            WeightedOccupancyForecastCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<WeightedOccupancyForecast> FindWeightedOccupancyForecasts(IWeightedOccupancyForecastStore store, WeightedOccupancyForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
