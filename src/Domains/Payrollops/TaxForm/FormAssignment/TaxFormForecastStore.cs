namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form forecasts by code.</summary>
public interface ITaxFormForecastStore
{
    TaxFormForecast? Find(string code);
    void Save(TaxFormForecast item);
    bool Remove(string code);
    List<TaxFormForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormForecastStore(Dictionary<string, TaxFormForecast> items, List<string> order) : ITaxFormForecastStore
{
    public Dictionary<string, TaxFormForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormForecast item)
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

    public List<TaxFormForecast> List()
    {
        var outValue = new List<TaxFormForecast>();
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
public sealed class CachedTaxFormForecastStore(MemoryTaxFormForecastStore inner, TaxFormForecastCache cache, int hits, int misses) : ITaxFormForecastStore
{
    public MemoryTaxFormForecastStore Inner { get; set; } = inner;
    public TaxFormForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormForecast? Find(string code)
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

    public void Save(TaxFormForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form forecasts. Empty fields match all.</summary>
public sealed record TaxFormForecastQuery(string Title, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormForecast> Apply(List<TaxFormForecast> items)
    {
        var matched = Collections.Filter(items, (TaxFormForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form forecast store.</summary>
public static class TaxFormForecastStore
{
    public static ITaxFormForecastStore NewMemoryTaxFormForecastStore()
    {
        return new MemoryTaxFormForecastStore(new Dictionary<string, TaxFormForecast>(), new List<string>());
    }

    public static ITaxFormForecastStore NewCachedTaxFormForecastStore(MemoryTaxFormForecastStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormForecastStore(inner, TaxFormForecastCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TaxFormForecast> FindTaxFormForecasts(ITaxFormForecastStore store, TaxFormForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
