namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped desk forecasts by code.</summary>
public interface ICappedDeskForecastStore
{
    CappedDeskForecast? Find(string code);
    void Save(CappedDeskForecast item);
    bool Remove(string code);
    List<CappedDeskForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedDeskForecastStore(Dictionary<string, CappedDeskForecast> items, List<string> order) : ICappedDeskForecastStore
{
    public Dictionary<string, CappedDeskForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedDeskForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedDeskForecast item)
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

    public List<CappedDeskForecast> List()
    {
        var outValue = new List<CappedDeskForecast>();
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
public sealed class CachedCappedDeskForecastStore(MemoryCappedDeskForecastStore inner, CappedDeskForecastCache cache, int hits, int misses) : ICappedDeskForecastStore
{
    public MemoryCappedDeskForecastStore Inner { get; set; } = inner;
    public CappedDeskForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedDeskForecast? Find(string code)
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

    public void Save(CappedDeskForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedDeskForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped desk forecasts. Empty fields match all.</summary>
public sealed record CappedDeskForecastQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(CappedDeskForecast item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<CappedDeskForecast> Apply(List<CappedDeskForecast> items)
    {
        var matched = Collections.Filter(items, (CappedDeskForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped desk forecast store.</summary>
public static class CappedDeskForecastStore
{
    public static ICappedDeskForecastStore NewMemoryCappedDeskForecastStore()
    {
        return new MemoryCappedDeskForecastStore(new Dictionary<string, CappedDeskForecast>(), new List<string>());
    }

    public static ICappedDeskForecastStore NewCachedCappedDeskForecastStore(MemoryCappedDeskForecastStore inner, int ttl, IClock clock)
    {
        return new CachedCappedDeskForecastStore(inner, CappedDeskForecastCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CappedDeskForecast> FindCappedDeskForecasts(ICappedDeskForecastStore store, CappedDeskForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
