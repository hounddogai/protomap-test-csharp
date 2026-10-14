namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict accrual forecasts by code.</summary>
public interface IStrictAccrualForecastStore
{
    StrictAccrualForecast? Find(string code);
    void Save(StrictAccrualForecast item);
    bool Remove(string code);
    List<StrictAccrualForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAccrualForecastStore(Dictionary<string, StrictAccrualForecast> items, List<string> order) : IStrictAccrualForecastStore
{
    public Dictionary<string, StrictAccrualForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAccrualForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAccrualForecast item)
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

    public List<StrictAccrualForecast> List()
    {
        var outValue = new List<StrictAccrualForecast>();
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
public sealed class CachedStrictAccrualForecastStore(MemoryStrictAccrualForecastStore inner, StrictAccrualForecastCache cache, int hits, int misses) : IStrictAccrualForecastStore
{
    public MemoryStrictAccrualForecastStore Inner { get; set; } = inner;
    public StrictAccrualForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAccrualForecast? Find(string code)
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

    public void Save(StrictAccrualForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAccrualForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict accrual forecasts. Empty fields match all.</summary>
public sealed record StrictAccrualForecastQuery(string Source, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAccrualForecast item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAccrualForecast> Apply(List<StrictAccrualForecast> items)
    {
        var matched = Collections.Filter(items, (StrictAccrualForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict accrual forecast store.</summary>
public static class StrictAccrualForecastStore
{
    public static IStrictAccrualForecastStore NewMemoryStrictAccrualForecastStore()
    {
        return new MemoryStrictAccrualForecastStore(
            new Dictionary<string, StrictAccrualForecast>(),
            new List<string>());
    }

    public static IStrictAccrualForecastStore NewCachedStrictAccrualForecastStore(MemoryStrictAccrualForecastStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAccrualForecastStore(inner, StrictAccrualForecastCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<StrictAccrualForecast> FindStrictAccrualForecasts(IStrictAccrualForecastStore store, StrictAccrualForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
