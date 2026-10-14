namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band forecasts by code.</summary>
public interface IPayBandForecastStore
{
    PayBandForecast? Find(string code);
    void Save(PayBandForecast item);
    bool Remove(string code);
    List<PayBandForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandForecastStore(Dictionary<string, PayBandForecast> items, List<string> order) : IPayBandForecastStore
{
    public Dictionary<string, PayBandForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandForecast item)
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

    public List<PayBandForecast> List()
    {
        var outValue = new List<PayBandForecast>();
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
public sealed class CachedPayBandForecastStore(MemoryPayBandForecastStore inner, PayBandForecastCache cache, int hits, int misses) : IPayBandForecastStore
{
    public MemoryPayBandForecastStore Inner { get; set; } = inner;
    public PayBandForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandForecast? Find(string code)
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

    public void Save(PayBandForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band forecasts. Empty fields match all.</summary>
public sealed record PayBandForecastQuery(string Department, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandForecast item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandForecast> Apply(List<PayBandForecast> items)
    {
        var matched = Collections.Filter(items, (PayBandForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band forecast store.</summary>
public static class PayBandForecastStore
{
    public static IPayBandForecastStore NewMemoryPayBandForecastStore()
    {
        return new MemoryPayBandForecastStore(new Dictionary<string, PayBandForecast>(), new List<string>());
    }

    public static IPayBandForecastStore NewCachedPayBandForecastStore(MemoryPayBandForecastStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandForecastStore(inner, PayBandForecastCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayBandForecast> FindPayBandForecasts(IPayBandForecastStore store, PayBandForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
