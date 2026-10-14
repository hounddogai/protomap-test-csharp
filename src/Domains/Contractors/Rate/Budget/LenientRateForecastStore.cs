namespace Acme.Workforce.Contractors.Rate.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient rate forecasts by code.</summary>
public interface ILenientRateForecastStore
{
    LenientRateForecast? Find(string code);
    void Save(LenientRateForecast item);
    bool Remove(string code);
    List<LenientRateForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientRateForecastStore(Dictionary<string, LenientRateForecast> items, List<string> order) : ILenientRateForecastStore
{
    public Dictionary<string, LenientRateForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientRateForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientRateForecast item)
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

    public List<LenientRateForecast> List()
    {
        var outValue = new List<LenientRateForecast>();
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
public sealed class CachedLenientRateForecastStore(MemoryLenientRateForecastStore inner, LenientRateForecastCache cache, int hits, int misses) : ILenientRateForecastStore
{
    public MemoryLenientRateForecastStore Inner { get; set; } = inner;
    public LenientRateForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientRateForecast? Find(string code)
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

    public void Save(LenientRateForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientRateForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient rate forecasts. Empty fields match all.</summary>
public sealed record LenientRateForecastQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(LenientRateForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientRateForecast> Apply(List<LenientRateForecast> items)
    {
        var matched = Collections.Filter(items, (LenientRateForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient rate forecast store.</summary>
public static class LenientRateForecastStore
{
    public static ILenientRateForecastStore NewMemoryLenientRateForecastStore()
    {
        return new MemoryLenientRateForecastStore(new Dictionary<string, LenientRateForecast>(), new List<string>());
    }

    public static ILenientRateForecastStore NewCachedLenientRateForecastStore(MemoryLenientRateForecastStore inner, int ttl, IClock clock)
    {
        return new CachedLenientRateForecastStore(inner, LenientRateForecastCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LenientRateForecast> FindLenientRateForecasts(ILenientRateForecastStore store, LenientRateForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
