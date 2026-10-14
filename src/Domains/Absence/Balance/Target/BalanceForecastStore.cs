namespace Acme.Workforce.Absence.Balance.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance forecasts by code.</summary>
public interface IBalanceForecastStore
{
    BalanceForecast? Find(string code);
    void Save(BalanceForecast item);
    bool Remove(string code);
    List<BalanceForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceForecastStore(Dictionary<string, BalanceForecast> items, List<string> order) : IBalanceForecastStore
{
    public Dictionary<string, BalanceForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceForecast item)
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

    public List<BalanceForecast> List()
    {
        var outValue = new List<BalanceForecast>();
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
public sealed class CachedBalanceForecastStore(MemoryBalanceForecastStore inner, BalanceForecastCache cache, int hits, int misses) : IBalanceForecastStore
{
    public MemoryBalanceForecastStore Inner { get; set; } = inner;
    public BalanceForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceForecast? Find(string code)
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

    public void Save(BalanceForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance forecasts. Empty fields match all.</summary>
public sealed record BalanceForecastQuery(string Region, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceForecast item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<BalanceForecast> Apply(List<BalanceForecast> items)
    {
        var matched = Collections.Filter(items, (BalanceForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance forecast store.</summary>
public static class BalanceForecastStore
{
    public static IBalanceForecastStore NewMemoryBalanceForecastStore()
    {
        return new MemoryBalanceForecastStore(new Dictionary<string, BalanceForecast>(), new List<string>());
    }

    public static IBalanceForecastStore NewCachedBalanceForecastStore(MemoryBalanceForecastStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceForecastStore(inner, BalanceForecastCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BalanceForecast> FindBalanceForecasts(IBalanceForecastStore store, BalanceForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
