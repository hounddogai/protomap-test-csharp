namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling mobility forecasts by code.</summary>
public interface IRollingMobilityForecastStore
{
    RollingMobilityForecast? Find(string code);
    void Save(RollingMobilityForecast item);
    bool Remove(string code);
    List<RollingMobilityForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingMobilityForecastStore(Dictionary<string, RollingMobilityForecast> items, List<string> order) : IRollingMobilityForecastStore
{
    public Dictionary<string, RollingMobilityForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingMobilityForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingMobilityForecast item)
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

    public List<RollingMobilityForecast> List()
    {
        var outValue = new List<RollingMobilityForecast>();
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
public sealed class CachedRollingMobilityForecastStore(MemoryRollingMobilityForecastStore inner, RollingMobilityForecastCache cache, int hits, int misses) : IRollingMobilityForecastStore
{
    public MemoryRollingMobilityForecastStore Inner { get; set; } = inner;
    public RollingMobilityForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingMobilityForecast? Find(string code)
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

    public void Save(RollingMobilityForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingMobilityForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling mobility forecasts. Empty fields match all.</summary>
public sealed record RollingMobilityForecastQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(RollingMobilityForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<RollingMobilityForecast> Apply(List<RollingMobilityForecast> items)
    {
        var matched = Collections.Filter(items, (RollingMobilityForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling mobility forecast store.</summary>
public static class RollingMobilityForecastStore
{
    public static IRollingMobilityForecastStore NewMemoryRollingMobilityForecastStore()
    {
        return new MemoryRollingMobilityForecastStore(
            new Dictionary<string, RollingMobilityForecast>(),
            new List<string>());
    }

    public static IRollingMobilityForecastStore NewCachedRollingMobilityForecastStore(MemoryRollingMobilityForecastStore inner, int ttl, IClock clock)
    {
        return new CachedRollingMobilityForecastStore(
            inner,
            RollingMobilityForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RollingMobilityForecast> FindRollingMobilityForecasts(IRollingMobilityForecastStore store, RollingMobilityForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
