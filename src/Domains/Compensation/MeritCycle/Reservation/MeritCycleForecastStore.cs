namespace Acme.Workforce.Compensation.MeritCycle.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle forecasts by code.</summary>
public interface IMeritCycleForecastStore
{
    MeritCycleForecast? Find(string code);
    void Save(MeritCycleForecast item);
    bool Remove(string code);
    List<MeritCycleForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleForecastStore(Dictionary<string, MeritCycleForecast> items, List<string> order) : IMeritCycleForecastStore
{
    public Dictionary<string, MeritCycleForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleForecast item)
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

    public List<MeritCycleForecast> List()
    {
        var outValue = new List<MeritCycleForecast>();
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
public sealed class CachedMeritCycleForecastStore(MemoryMeritCycleForecastStore inner, MeritCycleForecastCache cache, int hits, int misses) : IMeritCycleForecastStore
{
    public MemoryMeritCycleForecastStore Inner { get; set; } = inner;
    public MeritCycleForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleForecast? Find(string code)
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

    public void Save(MeritCycleForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle forecasts. Empty fields match all.</summary>
public sealed record MeritCycleForecastQuery(string Title, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleForecast> Apply(List<MeritCycleForecast> items)
    {
        var matched = Collections.Filter(items, (MeritCycleForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle forecast store.</summary>
public static class MeritCycleForecastStore
{
    public static IMeritCycleForecastStore NewMemoryMeritCycleForecastStore()
    {
        return new MemoryMeritCycleForecastStore(new Dictionary<string, MeritCycleForecast>(), new List<string>());
    }

    public static IMeritCycleForecastStore NewCachedMeritCycleForecastStore(MemoryMeritCycleForecastStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleForecastStore(inner, MeritCycleForecastCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<MeritCycleForecast> FindMeritCycleForecasts(IMeritCycleForecastStore store, MeritCycleForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
