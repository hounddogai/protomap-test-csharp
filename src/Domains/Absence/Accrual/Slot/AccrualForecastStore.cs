namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual forecasts by code.</summary>
public interface IAccrualForecastStore
{
    AccrualForecast? Find(string code);
    void Save(AccrualForecast item);
    bool Remove(string code);
    List<AccrualForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualForecastStore(Dictionary<string, AccrualForecast> items, List<string> order) : IAccrualForecastStore
{
    public Dictionary<string, AccrualForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualForecast item)
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

    public List<AccrualForecast> List()
    {
        var outValue = new List<AccrualForecast>();
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
public sealed class CachedAccrualForecastStore(MemoryAccrualForecastStore inner, AccrualForecastCache cache, int hits, int misses) : IAccrualForecastStore
{
    public MemoryAccrualForecastStore Inner { get; set; } = inner;
    public AccrualForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualForecast? Find(string code)
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

    public void Save(AccrualForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual forecasts. Empty fields match all.</summary>
public sealed record AccrualForecastQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualForecast item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualForecast> Apply(List<AccrualForecast> items)
    {
        var matched = Collections.Filter(items, (AccrualForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual forecast store.</summary>
public static class AccrualForecastStore
{
    public static IAccrualForecastStore NewMemoryAccrualForecastStore()
    {
        return new MemoryAccrualForecastStore(new Dictionary<string, AccrualForecast>(), new List<string>());
    }

    public static IAccrualForecastStore NewCachedAccrualForecastStore(MemoryAccrualForecastStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualForecastStore(inner, AccrualForecastCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AccrualForecast> FindAccrualForecasts(IAccrualForecastStore store, AccrualForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
