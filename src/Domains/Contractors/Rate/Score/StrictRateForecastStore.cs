namespace Acme.Workforce.Contractors.Rate.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict rate forecasts by code.</summary>
public interface IStrictRateForecastStore
{
    StrictRateForecast? Find(string code);
    void Save(StrictRateForecast item);
    bool Remove(string code);
    List<StrictRateForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictRateForecastStore(Dictionary<string, StrictRateForecast> items, List<string> order) : IStrictRateForecastStore
{
    public Dictionary<string, StrictRateForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictRateForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictRateForecast item)
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

    public List<StrictRateForecast> List()
    {
        var outValue = new List<StrictRateForecast>();
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
public sealed class CachedStrictRateForecastStore(MemoryStrictRateForecastStore inner, StrictRateForecastCache cache, int hits, int misses) : IStrictRateForecastStore
{
    public MemoryStrictRateForecastStore Inner { get; set; } = inner;
    public StrictRateForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictRateForecast? Find(string code)
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

    public void Save(StrictRateForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictRateForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict rate forecasts. Empty fields match all.</summary>
public sealed record StrictRateForecastQuery(string Unit, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(StrictRateForecast item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictRateForecast> Apply(List<StrictRateForecast> items)
    {
        var matched = Collections.Filter(items, (StrictRateForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict rate forecast store.</summary>
public static class StrictRateForecastStore
{
    public static IStrictRateForecastStore NewMemoryStrictRateForecastStore()
    {
        return new MemoryStrictRateForecastStore(new Dictionary<string, StrictRateForecast>(), new List<string>());
    }

    public static IStrictRateForecastStore NewCachedStrictRateForecastStore(MemoryStrictRateForecastStore inner, int ttl, IClock clock)
    {
        return new CachedStrictRateForecastStore(inner, StrictRateForecastCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<StrictRateForecast> FindStrictRateForecasts(IStrictRateForecastStore store, StrictRateForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
