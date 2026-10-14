namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment forecasts by code.</summary>
public interface IGarnishmentForecastStore
{
    GarnishmentForecast? Find(string code);
    void Save(GarnishmentForecast item);
    bool Remove(string code);
    List<GarnishmentForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentForecastStore(Dictionary<string, GarnishmentForecast> items, List<string> order) : IGarnishmentForecastStore
{
    public Dictionary<string, GarnishmentForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentForecast item)
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

    public List<GarnishmentForecast> List()
    {
        var outValue = new List<GarnishmentForecast>();
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
public sealed class CachedGarnishmentForecastStore(MemoryGarnishmentForecastStore inner, GarnishmentForecastCache cache, int hits, int misses) : IGarnishmentForecastStore
{
    public MemoryGarnishmentForecastStore Inner { get; set; } = inner;
    public GarnishmentForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentForecast? Find(string code)
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

    public void Save(GarnishmentForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment forecasts. Empty fields match all.</summary>
public sealed record GarnishmentForecastQuery(string Locale, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentForecast item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentForecast> Apply(List<GarnishmentForecast> items)
    {
        var matched = Collections.Filter(items, (GarnishmentForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment forecast store.</summary>
public static class GarnishmentForecastStore
{
    public static IGarnishmentForecastStore NewMemoryGarnishmentForecastStore()
    {
        return new MemoryGarnishmentForecastStore(new Dictionary<string, GarnishmentForecast>(), new List<string>());
    }

    public static IGarnishmentForecastStore NewCachedGarnishmentForecastStore(MemoryGarnishmentForecastStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentForecastStore(inner, GarnishmentForecastCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<GarnishmentForecast> FindGarnishmentForecasts(IGarnishmentForecastStore store, GarnishmentForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
