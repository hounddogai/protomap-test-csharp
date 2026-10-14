namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered nomination forecasts by code.</summary>
public interface ITieredNominationForecastStore
{
    TieredNominationForecast? Find(string code);
    void Save(TieredNominationForecast item);
    bool Remove(string code);
    List<TieredNominationForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredNominationForecastStore(Dictionary<string, TieredNominationForecast> items, List<string> order) : ITieredNominationForecastStore
{
    public Dictionary<string, TieredNominationForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredNominationForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredNominationForecast item)
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

    public List<TieredNominationForecast> List()
    {
        var outValue = new List<TieredNominationForecast>();
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
public sealed class CachedTieredNominationForecastStore(MemoryTieredNominationForecastStore inner, TieredNominationForecastCache cache, int hits, int misses) : ITieredNominationForecastStore
{
    public MemoryTieredNominationForecastStore Inner { get; set; } = inner;
    public TieredNominationForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredNominationForecast? Find(string code)
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

    public void Save(TieredNominationForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredNominationForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered nomination forecasts. Empty fields match all.</summary>
public sealed record TieredNominationForecastQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(TieredNominationForecast item)
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

    public List<TieredNominationForecast> Apply(List<TieredNominationForecast> items)
    {
        var matched = Collections.Filter(items, (TieredNominationForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered nomination forecast store.</summary>
public static class TieredNominationForecastStore
{
    public static ITieredNominationForecastStore NewMemoryTieredNominationForecastStore()
    {
        return new MemoryTieredNominationForecastStore(
            new Dictionary<string, TieredNominationForecast>(),
            new List<string>());
    }

    public static ITieredNominationForecastStore NewCachedTieredNominationForecastStore(MemoryTieredNominationForecastStore inner, int ttl, IClock clock)
    {
        return new CachedTieredNominationForecastStore(
            inner,
            TieredNominationForecastCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TieredNominationForecast> FindTieredNominationForecasts(ITieredNominationForecastStore store, TieredNominationForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
