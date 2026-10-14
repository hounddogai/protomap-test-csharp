namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy forecasts by code.</summary>
public interface IOccupancyForecastStore
{
    OccupancyForecast? Find(string code);
    void Save(OccupancyForecast item);
    bool Remove(string code);
    List<OccupancyForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyForecastStore(Dictionary<string, OccupancyForecast> items, List<string> order) : IOccupancyForecastStore
{
    public Dictionary<string, OccupancyForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyForecast item)
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

    public List<OccupancyForecast> List()
    {
        var outValue = new List<OccupancyForecast>();
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
public sealed class CachedOccupancyForecastStore(MemoryOccupancyForecastStore inner, OccupancyForecastCache cache, int hits, int misses) : IOccupancyForecastStore
{
    public MemoryOccupancyForecastStore Inner { get; set; } = inner;
    public OccupancyForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyForecast? Find(string code)
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

    public void Save(OccupancyForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy forecasts. Empty fields match all.</summary>
public sealed record OccupancyForecastQuery(string Team, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyForecast item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<OccupancyForecast> Apply(List<OccupancyForecast> items)
    {
        var matched = Collections.Filter(items, (OccupancyForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy forecast store.</summary>
public static class OccupancyForecastStore
{
    public static IOccupancyForecastStore NewMemoryOccupancyForecastStore()
    {
        return new MemoryOccupancyForecastStore(new Dictionary<string, OccupancyForecast>(), new List<string>());
    }

    public static IOccupancyForecastStore NewCachedOccupancyForecastStore(MemoryOccupancyForecastStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyForecastStore(inner, OccupancyForecastCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OccupancyForecast> FindOccupancyForecasts(IOccupancyForecastStore store, OccupancyForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
