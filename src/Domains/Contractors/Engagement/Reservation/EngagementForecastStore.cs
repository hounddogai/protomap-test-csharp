namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement forecasts by code.</summary>
public interface IEngagementForecastStore
{
    EngagementForecast? Find(string code);
    void Save(EngagementForecast item);
    bool Remove(string code);
    List<EngagementForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementForecastStore(Dictionary<string, EngagementForecast> items, List<string> order) : IEngagementForecastStore
{
    public Dictionary<string, EngagementForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementForecast item)
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

    public List<EngagementForecast> List()
    {
        var outValue = new List<EngagementForecast>();
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
public sealed class CachedEngagementForecastStore(MemoryEngagementForecastStore inner, EngagementForecastCache cache, int hits, int misses) : IEngagementForecastStore
{
    public MemoryEngagementForecastStore Inner { get; set; } = inner;
    public EngagementForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementForecast? Find(string code)
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

    public void Save(EngagementForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement forecasts. Empty fields match all.</summary>
public sealed record EngagementForecastQuery(string Label, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementForecast> Apply(List<EngagementForecast> items)
    {
        var matched = Collections.Filter(items, (EngagementForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement forecast store.</summary>
public static class EngagementForecastStore
{
    public static IEngagementForecastStore NewMemoryEngagementForecastStore()
    {
        return new MemoryEngagementForecastStore(new Dictionary<string, EngagementForecast>(), new List<string>());
    }

    public static IEngagementForecastStore NewCachedEngagementForecastStore(MemoryEngagementForecastStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementForecastStore(inner, EngagementForecastCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<EngagementForecast> FindEngagementForecasts(IEngagementForecastStore store, EngagementForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
