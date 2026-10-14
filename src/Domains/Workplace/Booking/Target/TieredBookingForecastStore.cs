namespace Acme.Workforce.Workplace.Booking.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered booking forecasts by code.</summary>
public interface ITieredBookingForecastStore
{
    TieredBookingForecast? Find(string code);
    void Save(TieredBookingForecast item);
    bool Remove(string code);
    List<TieredBookingForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredBookingForecastStore(Dictionary<string, TieredBookingForecast> items, List<string> order) : ITieredBookingForecastStore
{
    public Dictionary<string, TieredBookingForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredBookingForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredBookingForecast item)
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

    public List<TieredBookingForecast> List()
    {
        var outValue = new List<TieredBookingForecast>();
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
public sealed class CachedTieredBookingForecastStore(MemoryTieredBookingForecastStore inner, TieredBookingForecastCache cache, int hits, int misses) : ITieredBookingForecastStore
{
    public MemoryTieredBookingForecastStore Inner { get; set; } = inner;
    public TieredBookingForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredBookingForecast? Find(string code)
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

    public void Save(TieredBookingForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredBookingForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered booking forecasts. Empty fields match all.</summary>
public sealed record TieredBookingForecastQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(TieredBookingForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredBookingForecast> Apply(List<TieredBookingForecast> items)
    {
        var matched = Collections.Filter(items, (TieredBookingForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered booking forecast store.</summary>
public static class TieredBookingForecastStore
{
    public static ITieredBookingForecastStore NewMemoryTieredBookingForecastStore()
    {
        return new MemoryTieredBookingForecastStore(
            new Dictionary<string, TieredBookingForecast>(),
            new List<string>());
    }

    public static ITieredBookingForecastStore NewCachedTieredBookingForecastStore(MemoryTieredBookingForecastStore inner, int ttl, IClock clock)
    {
        return new CachedTieredBookingForecastStore(inner, TieredBookingForecastCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TieredBookingForecast> FindTieredBookingForecasts(ITieredBookingForecastStore store, TieredBookingForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
