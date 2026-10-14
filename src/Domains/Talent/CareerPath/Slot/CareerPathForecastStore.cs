namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path forecasts by code.</summary>
public interface ICareerPathForecastStore
{
    CareerPathForecast? Find(string code);
    void Save(CareerPathForecast item);
    bool Remove(string code);
    List<CareerPathForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathForecastStore(Dictionary<string, CareerPathForecast> items, List<string> order) : ICareerPathForecastStore
{
    public Dictionary<string, CareerPathForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathForecast item)
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

    public List<CareerPathForecast> List()
    {
        var outValue = new List<CareerPathForecast>();
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
public sealed class CachedCareerPathForecastStore(MemoryCareerPathForecastStore inner, CareerPathForecastCache cache, int hits, int misses) : ICareerPathForecastStore
{
    public MemoryCareerPathForecastStore Inner { get; set; } = inner;
    public CareerPathForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathForecast? Find(string code)
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

    public void Save(CareerPathForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path forecasts. Empty fields match all.</summary>
public sealed record CareerPathForecastQuery(string Locale, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathForecast item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathForecast> Apply(List<CareerPathForecast> items)
    {
        var matched = Collections.Filter(items, (CareerPathForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path forecast store.</summary>
public static class CareerPathForecastStore
{
    public static ICareerPathForecastStore NewMemoryCareerPathForecastStore()
    {
        return new MemoryCareerPathForecastStore(new Dictionary<string, CareerPathForecast>(), new List<string>());
    }

    public static ICareerPathForecastStore NewCachedCareerPathForecastStore(MemoryCareerPathForecastStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathForecastStore(inner, CareerPathForecastCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CareerPathForecast> FindCareerPathForecasts(ICareerPathForecastStore store, CareerPathForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
