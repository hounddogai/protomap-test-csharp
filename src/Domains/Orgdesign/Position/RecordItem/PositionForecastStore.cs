namespace Acme.Workforce.Orgdesign.Position.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position forecasts by code.</summary>
public interface IPositionForecastStore
{
    PositionForecast? Find(string code);
    void Save(PositionForecast item);
    bool Remove(string code);
    List<PositionForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionForecastStore(Dictionary<string, PositionForecast> items, List<string> order) : IPositionForecastStore
{
    public Dictionary<string, PositionForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionForecast item)
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

    public List<PositionForecast> List()
    {
        var outValue = new List<PositionForecast>();
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
public sealed class CachedPositionForecastStore(MemoryPositionForecastStore inner, PositionForecastCache cache, int hits, int misses) : IPositionForecastStore
{
    public MemoryPositionForecastStore Inner { get; set; } = inner;
    public PositionForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionForecast? Find(string code)
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

    public void Save(PositionForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position forecasts. Empty fields match all.</summary>
public sealed record PositionForecastQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PositionForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionForecast> Apply(List<PositionForecast> items)
    {
        var matched = Collections.Filter(items, (PositionForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position forecast store.</summary>
public static class PositionForecastStore
{
    public static IPositionForecastStore NewMemoryPositionForecastStore()
    {
        return new MemoryPositionForecastStore(new Dictionary<string, PositionForecast>(), new List<string>());
    }

    public static IPositionForecastStore NewCachedPositionForecastStore(MemoryPositionForecastStore inner, int ttl, IClock clock)
    {
        return new CachedPositionForecastStore(inner, PositionForecastCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PositionForecast> FindPositionForecasts(IPositionForecastStore store, PositionForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
