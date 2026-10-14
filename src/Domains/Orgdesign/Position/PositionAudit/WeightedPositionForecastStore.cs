namespace Acme.Workforce.Orgdesign.Position.PositionAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted position forecasts by code.</summary>
public interface IWeightedPositionForecastStore
{
    WeightedPositionForecast? Find(string code);
    void Save(WeightedPositionForecast item);
    bool Remove(string code);
    List<WeightedPositionForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedPositionForecastStore(Dictionary<string, WeightedPositionForecast> items, List<string> order) : IWeightedPositionForecastStore
{
    public Dictionary<string, WeightedPositionForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedPositionForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedPositionForecast item)
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

    public List<WeightedPositionForecast> List()
    {
        var outValue = new List<WeightedPositionForecast>();
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
public sealed class CachedWeightedPositionForecastStore(MemoryWeightedPositionForecastStore inner, WeightedPositionForecastCache cache, int hits, int misses) : IWeightedPositionForecastStore
{
    public MemoryWeightedPositionForecastStore Inner { get; set; } = inner;
    public WeightedPositionForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedPositionForecast? Find(string code)
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

    public void Save(WeightedPositionForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedPositionForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted position forecasts. Empty fields match all.</summary>
public sealed record WeightedPositionForecastQuery(string Source, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedPositionForecast item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedPositionForecast> Apply(List<WeightedPositionForecast> items)
    {
        var matched = Collections.Filter(items, (WeightedPositionForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted position forecast store.</summary>
public static class WeightedPositionForecastStore
{
    public static IWeightedPositionForecastStore NewMemoryWeightedPositionForecastStore()
    {
        return new MemoryWeightedPositionForecastStore(
            new Dictionary<string, WeightedPositionForecast>(),
            new List<string>());
    }

    public static IWeightedPositionForecastStore NewCachedWeightedPositionForecastStore(MemoryWeightedPositionForecastStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedPositionForecastStore(
            inner,
            WeightedPositionForecastCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedPositionForecast> FindWeightedPositionForecasts(IWeightedPositionForecastStore store, WeightedPositionForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
