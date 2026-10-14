namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary desk forecasts by code.</summary>
public interface IPrimaryDeskForecastStore
{
    PrimaryDeskForecast? Find(string code);
    void Save(PrimaryDeskForecast item);
    bool Remove(string code);
    List<PrimaryDeskForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryDeskForecastStore(Dictionary<string, PrimaryDeskForecast> items, List<string> order) : IPrimaryDeskForecastStore
{
    public Dictionary<string, PrimaryDeskForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryDeskForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryDeskForecast item)
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

    public List<PrimaryDeskForecast> List()
    {
        var outValue = new List<PrimaryDeskForecast>();
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
public sealed class CachedPrimaryDeskForecastStore(MemoryPrimaryDeskForecastStore inner, PrimaryDeskForecastCache cache, int hits, int misses) : IPrimaryDeskForecastStore
{
    public MemoryPrimaryDeskForecastStore Inner { get; set; } = inner;
    public PrimaryDeskForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryDeskForecast? Find(string code)
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

    public void Save(PrimaryDeskForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryDeskForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary desk forecasts. Empty fields match all.</summary>
public sealed record PrimaryDeskForecastQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryDeskForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryDeskForecast> Apply(List<PrimaryDeskForecast> items)
    {
        var matched = Collections.Filter(items, (PrimaryDeskForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary desk forecast store.</summary>
public static class PrimaryDeskForecastStore
{
    public static IPrimaryDeskForecastStore NewMemoryPrimaryDeskForecastStore()
    {
        return new MemoryPrimaryDeskForecastStore(new Dictionary<string, PrimaryDeskForecast>(), new List<string>());
    }

    public static IPrimaryDeskForecastStore NewCachedPrimaryDeskForecastStore(MemoryPrimaryDeskForecastStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryDeskForecastStore(inner, PrimaryDeskForecastCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PrimaryDeskForecast> FindPrimaryDeskForecasts(IPrimaryDeskForecastStore store, PrimaryDeskForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
