namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled tax form forecasts by code.</summary>
public interface IScheduledTaxFormForecastStore
{
    ScheduledTaxFormForecast? Find(string code);
    void Save(ScheduledTaxFormForecast item);
    bool Remove(string code);
    List<ScheduledTaxFormForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledTaxFormForecastStore(Dictionary<string, ScheduledTaxFormForecast> items, List<string> order) : IScheduledTaxFormForecastStore
{
    public Dictionary<string, ScheduledTaxFormForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledTaxFormForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledTaxFormForecast item)
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

    public List<ScheduledTaxFormForecast> List()
    {
        var outValue = new List<ScheduledTaxFormForecast>();
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
public sealed class CachedScheduledTaxFormForecastStore(MemoryScheduledTaxFormForecastStore inner, ScheduledTaxFormForecastCache cache, int hits, int misses) : IScheduledTaxFormForecastStore
{
    public MemoryScheduledTaxFormForecastStore Inner { get; set; } = inner;
    public ScheduledTaxFormForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledTaxFormForecast? Find(string code)
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

    public void Save(ScheduledTaxFormForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledTaxFormForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled tax form forecasts. Empty fields match all.</summary>
public sealed record ScheduledTaxFormForecastQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledTaxFormForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledTaxFormForecast> Apply(List<ScheduledTaxFormForecast> items)
    {
        var matched = Collections.Filter(items, (ScheduledTaxFormForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled tax form forecast store.</summary>
public static class ScheduledTaxFormForecastStore
{
    public static IScheduledTaxFormForecastStore NewMemoryScheduledTaxFormForecastStore()
    {
        return new MemoryScheduledTaxFormForecastStore(
            new Dictionary<string, ScheduledTaxFormForecast>(),
            new List<string>());
    }

    public static IScheduledTaxFormForecastStore NewCachedScheduledTaxFormForecastStore(MemoryScheduledTaxFormForecastStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledTaxFormForecastStore(
            inner,
            ScheduledTaxFormForecastCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledTaxFormForecast> FindScheduledTaxFormForecasts(IScheduledTaxFormForecastStore store, ScheduledTaxFormForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
