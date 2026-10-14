namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span forecasts by code.</summary>
public interface ISpanForecastStore
{
    SpanForecast? Find(string code);
    void Save(SpanForecast item);
    bool Remove(string code);
    List<SpanForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanForecastStore(Dictionary<string, SpanForecast> items, List<string> order) : ISpanForecastStore
{
    public Dictionary<string, SpanForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanForecast item)
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

    public List<SpanForecast> List()
    {
        var outValue = new List<SpanForecast>();
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
public sealed class CachedSpanForecastStore(MemorySpanForecastStore inner, SpanForecastCache cache, int hits, int misses) : ISpanForecastStore
{
    public MemorySpanForecastStore Inner { get; set; } = inner;
    public SpanForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanForecast? Find(string code)
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

    public void Save(SpanForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span forecasts. Empty fields match all.</summary>
public sealed record SpanForecastQuery(string Category, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(SpanForecast item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<SpanForecast> Apply(List<SpanForecast> items)
    {
        var matched = Collections.Filter(items, (SpanForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span forecast store.</summary>
public static class SpanForecastStore
{
    public static ISpanForecastStore NewMemorySpanForecastStore()
    {
        return new MemorySpanForecastStore(new Dictionary<string, SpanForecast>(), new List<string>());
    }

    public static ISpanForecastStore NewCachedSpanForecastStore(MemorySpanForecastStore inner, int ttl, IClock clock)
    {
        return new CachedSpanForecastStore(inner, SpanForecastCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SpanForecast> FindSpanForecasts(ISpanForecastStore store, SpanForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
