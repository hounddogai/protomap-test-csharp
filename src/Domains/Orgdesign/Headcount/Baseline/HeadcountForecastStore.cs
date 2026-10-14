namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount forecasts by code.</summary>
public interface IHeadcountForecastStore
{
    HeadcountForecast? Find(string code);
    void Save(HeadcountForecast item);
    bool Remove(string code);
    List<HeadcountForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountForecastStore(Dictionary<string, HeadcountForecast> items, List<string> order) : IHeadcountForecastStore
{
    public Dictionary<string, HeadcountForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountForecast item)
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

    public List<HeadcountForecast> List()
    {
        var outValue = new List<HeadcountForecast>();
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
public sealed class CachedHeadcountForecastStore(MemoryHeadcountForecastStore inner, HeadcountForecastCache cache, int hits, int misses) : IHeadcountForecastStore
{
    public MemoryHeadcountForecastStore Inner { get; set; } = inner;
    public HeadcountForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountForecast? Find(string code)
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

    public void Save(HeadcountForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount forecasts. Empty fields match all.</summary>
public sealed record HeadcountForecastQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountForecast item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountForecast> Apply(List<HeadcountForecast> items)
    {
        var matched = Collections.Filter(items, (HeadcountForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount forecast store.</summary>
public static class HeadcountForecastStore
{
    public static IHeadcountForecastStore NewMemoryHeadcountForecastStore()
    {
        return new MemoryHeadcountForecastStore(new Dictionary<string, HeadcountForecast>(), new List<string>());
    }

    public static IHeadcountForecastStore NewCachedHeadcountForecastStore(MemoryHeadcountForecastStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountForecastStore(inner, HeadcountForecastCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HeadcountForecast> FindHeadcountForecasts(IHeadcountForecastStore store, HeadcountForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
