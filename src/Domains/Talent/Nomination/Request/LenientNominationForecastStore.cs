namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient nomination forecasts by code.</summary>
public interface ILenientNominationForecastStore
{
    LenientNominationForecast? Find(string code);
    void Save(LenientNominationForecast item);
    bool Remove(string code);
    List<LenientNominationForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientNominationForecastStore(Dictionary<string, LenientNominationForecast> items, List<string> order) : ILenientNominationForecastStore
{
    public Dictionary<string, LenientNominationForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientNominationForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientNominationForecast item)
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

    public List<LenientNominationForecast> List()
    {
        var outValue = new List<LenientNominationForecast>();
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
public sealed class CachedLenientNominationForecastStore(MemoryLenientNominationForecastStore inner, LenientNominationForecastCache cache, int hits, int misses) : ILenientNominationForecastStore
{
    public MemoryLenientNominationForecastStore Inner { get; set; } = inner;
    public LenientNominationForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientNominationForecast? Find(string code)
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

    public void Save(LenientNominationForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientNominationForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient nomination forecasts. Empty fields match all.</summary>
public sealed record LenientNominationForecastQuery(string Unit, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(LenientNominationForecast item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientNominationForecast> Apply(List<LenientNominationForecast> items)
    {
        var matched = Collections.Filter(items, (LenientNominationForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient nomination forecast store.</summary>
public static class LenientNominationForecastStore
{
    public static ILenientNominationForecastStore NewMemoryLenientNominationForecastStore()
    {
        return new MemoryLenientNominationForecastStore(
            new Dictionary<string, LenientNominationForecast>(),
            new List<string>());
    }

    public static ILenientNominationForecastStore NewCachedLenientNominationForecastStore(MemoryLenientNominationForecastStore inner, int ttl, IClock clock)
    {
        return new CachedLenientNominationForecastStore(
            inner,
            LenientNominationForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientNominationForecast> FindLenientNominationForecasts(ILenientNominationForecastStore store, LenientNominationForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
