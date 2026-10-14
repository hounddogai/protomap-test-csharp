namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary pay band forecasts by code.</summary>
public interface IPrimaryPayBandForecastStore
{
    PrimaryPayBandForecast? Find(string code);
    void Save(PrimaryPayBandForecast item);
    bool Remove(string code);
    List<PrimaryPayBandForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPayBandForecastStore(Dictionary<string, PrimaryPayBandForecast> items, List<string> order) : IPrimaryPayBandForecastStore
{
    public Dictionary<string, PrimaryPayBandForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPayBandForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPayBandForecast item)
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

    public List<PrimaryPayBandForecast> List()
    {
        var outValue = new List<PrimaryPayBandForecast>();
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
public sealed class CachedPrimaryPayBandForecastStore(MemoryPrimaryPayBandForecastStore inner, PrimaryPayBandForecastCache cache, int hits, int misses) : IPrimaryPayBandForecastStore
{
    public MemoryPrimaryPayBandForecastStore Inner { get; set; } = inner;
    public PrimaryPayBandForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPayBandForecast? Find(string code)
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

    public void Save(PrimaryPayBandForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPayBandForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary pay band forecasts. Empty fields match all.</summary>
public sealed record PrimaryPayBandForecastQuery(string Label, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPayBandForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryPayBandForecast> Apply(List<PrimaryPayBandForecast> items)
    {
        var matched = Collections.Filter(items, (PrimaryPayBandForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary pay band forecast store.</summary>
public static class PrimaryPayBandForecastStore
{
    public static IPrimaryPayBandForecastStore NewMemoryPrimaryPayBandForecastStore()
    {
        return new MemoryPrimaryPayBandForecastStore(
            new Dictionary<string, PrimaryPayBandForecast>(),
            new List<string>());
    }

    public static IPrimaryPayBandForecastStore NewCachedPrimaryPayBandForecastStore(MemoryPrimaryPayBandForecastStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPayBandForecastStore(
            inner,
            PrimaryPayBandForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryPayBandForecast> FindPrimaryPayBandForecasts(IPrimaryPayBandForecastStore store, PrimaryPayBandForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
