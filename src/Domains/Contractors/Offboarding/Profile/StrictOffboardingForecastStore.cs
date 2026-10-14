namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict offboarding forecasts by code.</summary>
public interface IStrictOffboardingForecastStore
{
    StrictOffboardingForecast? Find(string code);
    void Save(StrictOffboardingForecast item);
    bool Remove(string code);
    List<StrictOffboardingForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictOffboardingForecastStore(Dictionary<string, StrictOffboardingForecast> items, List<string> order) : IStrictOffboardingForecastStore
{
    public Dictionary<string, StrictOffboardingForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictOffboardingForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictOffboardingForecast item)
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

    public List<StrictOffboardingForecast> List()
    {
        var outValue = new List<StrictOffboardingForecast>();
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
public sealed class CachedStrictOffboardingForecastStore(MemoryStrictOffboardingForecastStore inner, StrictOffboardingForecastCache cache, int hits, int misses) : IStrictOffboardingForecastStore
{
    public MemoryStrictOffboardingForecastStore Inner { get; set; } = inner;
    public StrictOffboardingForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictOffboardingForecast? Find(string code)
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

    public void Save(StrictOffboardingForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictOffboardingForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict offboarding forecasts. Empty fields match all.</summary>
public sealed record StrictOffboardingForecastQuery(string Title, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(StrictOffboardingForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictOffboardingForecast> Apply(List<StrictOffboardingForecast> items)
    {
        var matched = Collections.Filter(items, (StrictOffboardingForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict offboarding forecast store.</summary>
public static class StrictOffboardingForecastStore
{
    public static IStrictOffboardingForecastStore NewMemoryStrictOffboardingForecastStore()
    {
        return new MemoryStrictOffboardingForecastStore(
            new Dictionary<string, StrictOffboardingForecast>(),
            new List<string>());
    }

    public static IStrictOffboardingForecastStore NewCachedStrictOffboardingForecastStore(MemoryStrictOffboardingForecastStore inner, int ttl, IClock clock)
    {
        return new CachedStrictOffboardingForecastStore(
            inner,
            StrictOffboardingForecastCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictOffboardingForecast> FindStrictOffboardingForecasts(IStrictOffboardingForecastStore store, StrictOffboardingForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
