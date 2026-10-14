namespace Acme.Workforce.Talent.SuccessionPlan.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan forecasts by code.</summary>
public interface ISuccessionPlanForecastStore
{
    SuccessionPlanForecast? Find(string code);
    void Save(SuccessionPlanForecast item);
    bool Remove(string code);
    List<SuccessionPlanForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanForecastStore(Dictionary<string, SuccessionPlanForecast> items, List<string> order) : ISuccessionPlanForecastStore
{
    public Dictionary<string, SuccessionPlanForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanForecast item)
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

    public List<SuccessionPlanForecast> List()
    {
        var outValue = new List<SuccessionPlanForecast>();
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
public sealed class CachedSuccessionPlanForecastStore(MemorySuccessionPlanForecastStore inner, SuccessionPlanForecastCache cache, int hits, int misses) : ISuccessionPlanForecastStore
{
    public MemorySuccessionPlanForecastStore Inner { get; set; } = inner;
    public SuccessionPlanForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanForecast? Find(string code)
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

    public void Save(SuccessionPlanForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan forecasts. Empty fields match all.</summary>
public sealed record SuccessionPlanForecastQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<SuccessionPlanForecast> Apply(List<SuccessionPlanForecast> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan forecast store.</summary>
public static class SuccessionPlanForecastStore
{
    public static ISuccessionPlanForecastStore NewMemorySuccessionPlanForecastStore()
    {
        return new MemorySuccessionPlanForecastStore(
            new Dictionary<string, SuccessionPlanForecast>(),
            new List<string>());
    }

    public static ISuccessionPlanForecastStore NewCachedSuccessionPlanForecastStore(MemorySuccessionPlanForecastStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanForecastStore(
            inner,
            SuccessionPlanForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<SuccessionPlanForecast> FindSuccessionPlanForecasts(ISuccessionPlanForecastStore store, SuccessionPlanForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
