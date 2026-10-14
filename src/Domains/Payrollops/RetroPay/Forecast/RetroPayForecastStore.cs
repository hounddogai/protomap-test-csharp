namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay forecasts by code.</summary>
public interface IRetroPayForecastStore
{
    RetroPayForecast? Find(string code);
    void Save(RetroPayForecast item);
    bool Remove(string code);
    List<RetroPayForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayForecastStore(Dictionary<string, RetroPayForecast> items, List<string> order) : IRetroPayForecastStore
{
    public Dictionary<string, RetroPayForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayForecast item)
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

    public List<RetroPayForecast> List()
    {
        var outValue = new List<RetroPayForecast>();
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
public sealed class CachedRetroPayForecastStore(MemoryRetroPayForecastStore inner, RetroPayForecastCache cache, int hits, int misses) : IRetroPayForecastStore
{
    public MemoryRetroPayForecastStore Inner { get; set; } = inner;
    public RetroPayForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayForecast? Find(string code)
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

    public void Save(RetroPayForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay forecasts. Empty fields match all.</summary>
public sealed record RetroPayForecastQuery(string Region, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayForecast item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayForecast> Apply(List<RetroPayForecast> items)
    {
        var matched = Collections.Filter(items, (RetroPayForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay forecast store.</summary>
public static class RetroPayForecastStore
{
    public static IRetroPayForecastStore NewMemoryRetroPayForecastStore()
    {
        return new MemoryRetroPayForecastStore(new Dictionary<string, RetroPayForecast>(), new List<string>());
    }

    public static IRetroPayForecastStore NewCachedRetroPayForecastStore(MemoryRetroPayForecastStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayForecastStore(inner, RetroPayForecastCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RetroPayForecast> FindRetroPayForecasts(IRetroPayForecastStore store, RetroPayForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
