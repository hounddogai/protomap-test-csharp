namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global pool forecasts by code.</summary>
public interface IGlobalPoolForecastStore
{
    GlobalPoolForecast? Find(string code);
    void Save(GlobalPoolForecast item);
    bool Remove(string code);
    List<GlobalPoolForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPoolForecastStore(Dictionary<string, GlobalPoolForecast> items, List<string> order) : IGlobalPoolForecastStore
{
    public Dictionary<string, GlobalPoolForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPoolForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPoolForecast item)
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

    public List<GlobalPoolForecast> List()
    {
        var outValue = new List<GlobalPoolForecast>();
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
public sealed class CachedGlobalPoolForecastStore(MemoryGlobalPoolForecastStore inner, GlobalPoolForecastCache cache, int hits, int misses) : IGlobalPoolForecastStore
{
    public MemoryGlobalPoolForecastStore Inner { get; set; } = inner;
    public GlobalPoolForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPoolForecast? Find(string code)
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

    public void Save(GlobalPoolForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPoolForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global pool forecasts. Empty fields match all.</summary>
public sealed record GlobalPoolForecastQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPoolForecast item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPoolForecast> Apply(List<GlobalPoolForecast> items)
    {
        var matched = Collections.Filter(items, (GlobalPoolForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global pool forecast store.</summary>
public static class GlobalPoolForecastStore
{
    public static IGlobalPoolForecastStore NewMemoryGlobalPoolForecastStore()
    {
        return new MemoryGlobalPoolForecastStore(new Dictionary<string, GlobalPoolForecast>(), new List<string>());
    }

    public static IGlobalPoolForecastStore NewCachedGlobalPoolForecastStore(MemoryGlobalPoolForecastStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPoolForecastStore(inner, GlobalPoolForecastCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<GlobalPoolForecast> FindGlobalPoolForecasts(IGlobalPoolForecastStore store, GlobalPoolForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
