namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive pool forecasts by code.</summary>
public interface IAdaptivePoolForecastStore
{
    AdaptivePoolForecast? Find(string code);
    void Save(AdaptivePoolForecast item);
    bool Remove(string code);
    List<AdaptivePoolForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptivePoolForecastStore(Dictionary<string, AdaptivePoolForecast> items, List<string> order) : IAdaptivePoolForecastStore
{
    public Dictionary<string, AdaptivePoolForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptivePoolForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptivePoolForecast item)
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

    public List<AdaptivePoolForecast> List()
    {
        var outValue = new List<AdaptivePoolForecast>();
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
public sealed class CachedAdaptivePoolForecastStore(MemoryAdaptivePoolForecastStore inner, AdaptivePoolForecastCache cache, int hits, int misses) : IAdaptivePoolForecastStore
{
    public MemoryAdaptivePoolForecastStore Inner { get; set; } = inner;
    public AdaptivePoolForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptivePoolForecast? Find(string code)
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

    public void Save(AdaptivePoolForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptivePoolForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive pool forecasts. Empty fields match all.</summary>
public sealed record AdaptivePoolForecastQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptivePoolForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptivePoolForecast> Apply(List<AdaptivePoolForecast> items)
    {
        var matched = Collections.Filter(items, (AdaptivePoolForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive pool forecast store.</summary>
public static class AdaptivePoolForecastStore
{
    public static IAdaptivePoolForecastStore NewMemoryAdaptivePoolForecastStore()
    {
        return new MemoryAdaptivePoolForecastStore(
            new Dictionary<string, AdaptivePoolForecast>(),
            new List<string>());
    }

    public static IAdaptivePoolForecastStore NewCachedAdaptivePoolForecastStore(MemoryAdaptivePoolForecastStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptivePoolForecastStore(inner, AdaptivePoolForecastCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AdaptivePoolForecast> FindAdaptivePoolForecasts(IAdaptivePoolForecastStore store, AdaptivePoolForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
