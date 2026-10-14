namespace Acme.Workforce.Compensation.Adjustment.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment forecasts by code.</summary>
public interface IAdjustmentForecastStore
{
    AdjustmentForecast? Find(string code);
    void Save(AdjustmentForecast item);
    bool Remove(string code);
    List<AdjustmentForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentForecastStore(Dictionary<string, AdjustmentForecast> items, List<string> order) : IAdjustmentForecastStore
{
    public Dictionary<string, AdjustmentForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentForecast item)
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

    public List<AdjustmentForecast> List()
    {
        var outValue = new List<AdjustmentForecast>();
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
public sealed class CachedAdjustmentForecastStore(MemoryAdjustmentForecastStore inner, AdjustmentForecastCache cache, int hits, int misses) : IAdjustmentForecastStore
{
    public MemoryAdjustmentForecastStore Inner { get; set; } = inner;
    public AdjustmentForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentForecast? Find(string code)
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

    public void Save(AdjustmentForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment forecasts. Empty fields match all.</summary>
public sealed record AdjustmentForecastQuery(string Unit, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentForecast item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentForecast> Apply(List<AdjustmentForecast> items)
    {
        var matched = Collections.Filter(items, (AdjustmentForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment forecast store.</summary>
public static class AdjustmentForecastStore
{
    public static IAdjustmentForecastStore NewMemoryAdjustmentForecastStore()
    {
        return new MemoryAdjustmentForecastStore(new Dictionary<string, AdjustmentForecast>(), new List<string>());
    }

    public static IAdjustmentForecastStore NewCachedAdjustmentForecastStore(MemoryAdjustmentForecastStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentForecastStore(inner, AdjustmentForecastCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AdjustmentForecast> FindAdjustmentForecasts(IAdjustmentForecastStore store, AdjustmentForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
