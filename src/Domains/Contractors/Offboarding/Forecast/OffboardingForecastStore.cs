namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding forecasts by code.</summary>
public interface IOffboardingForecastStore
{
    OffboardingForecast? Find(string code);
    void Save(OffboardingForecast item);
    bool Remove(string code);
    List<OffboardingForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingForecastStore(Dictionary<string, OffboardingForecast> items, List<string> order) : IOffboardingForecastStore
{
    public Dictionary<string, OffboardingForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingForecast item)
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

    public List<OffboardingForecast> List()
    {
        var outValue = new List<OffboardingForecast>();
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
public sealed class CachedOffboardingForecastStore(MemoryOffboardingForecastStore inner, OffboardingForecastCache cache, int hits, int misses) : IOffboardingForecastStore
{
    public MemoryOffboardingForecastStore Inner { get; set; } = inner;
    public OffboardingForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingForecast? Find(string code)
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

    public void Save(OffboardingForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding forecasts. Empty fields match all.</summary>
public sealed record OffboardingForecastQuery(string Country, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingForecast item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingForecast> Apply(List<OffboardingForecast> items)
    {
        var matched = Collections.Filter(items, (OffboardingForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding forecast store.</summary>
public static class OffboardingForecastStore
{
    public static IOffboardingForecastStore NewMemoryOffboardingForecastStore()
    {
        return new MemoryOffboardingForecastStore(new Dictionary<string, OffboardingForecast>(), new List<string>());
    }

    public static IOffboardingForecastStore NewCachedOffboardingForecastStore(MemoryOffboardingForecastStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingForecastStore(inner, OffboardingForecastCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<OffboardingForecast> FindOffboardingForecasts(IOffboardingForecastStore store, OffboardingForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
