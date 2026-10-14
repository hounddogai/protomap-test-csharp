namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global occupancy forecasts by code.</summary>
public interface IGlobalOccupancyForecastStore
{
    GlobalOccupancyForecast? Find(string code);
    void Save(GlobalOccupancyForecast item);
    bool Remove(string code);
    List<GlobalOccupancyForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalOccupancyForecastStore(Dictionary<string, GlobalOccupancyForecast> items, List<string> order) : IGlobalOccupancyForecastStore
{
    public Dictionary<string, GlobalOccupancyForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalOccupancyForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalOccupancyForecast item)
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

    public List<GlobalOccupancyForecast> List()
    {
        var outValue = new List<GlobalOccupancyForecast>();
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
public sealed class CachedGlobalOccupancyForecastStore(MemoryGlobalOccupancyForecastStore inner, GlobalOccupancyForecastCache cache, int hits, int misses) : IGlobalOccupancyForecastStore
{
    public MemoryGlobalOccupancyForecastStore Inner { get; set; } = inner;
    public GlobalOccupancyForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalOccupancyForecast? Find(string code)
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

    public void Save(GlobalOccupancyForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalOccupancyForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global occupancy forecasts. Empty fields match all.</summary>
public sealed record GlobalOccupancyForecastQuery(string Source, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalOccupancyForecast item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalOccupancyForecast> Apply(List<GlobalOccupancyForecast> items)
    {
        var matched = Collections.Filter(items, (GlobalOccupancyForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global occupancy forecast store.</summary>
public static class GlobalOccupancyForecastStore
{
    public static IGlobalOccupancyForecastStore NewMemoryGlobalOccupancyForecastStore()
    {
        return new MemoryGlobalOccupancyForecastStore(
            new Dictionary<string, GlobalOccupancyForecast>(),
            new List<string>());
    }

    public static IGlobalOccupancyForecastStore NewCachedGlobalOccupancyForecastStore(MemoryGlobalOccupancyForecastStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalOccupancyForecastStore(
            inner,
            GlobalOccupancyForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<GlobalOccupancyForecast> FindGlobalOccupancyForecasts(IGlobalOccupancyForecastStore store, GlobalOccupancyForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
