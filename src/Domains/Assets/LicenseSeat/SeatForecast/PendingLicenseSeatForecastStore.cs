namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending license seat forecasts by code.</summary>
public interface IPendingLicenseSeatForecastStore
{
    PendingLicenseSeatForecast? Find(string code);
    void Save(PendingLicenseSeatForecast item);
    bool Remove(string code);
    List<PendingLicenseSeatForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingLicenseSeatForecastStore(Dictionary<string, PendingLicenseSeatForecast> items, List<string> order) : IPendingLicenseSeatForecastStore
{
    public Dictionary<string, PendingLicenseSeatForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingLicenseSeatForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingLicenseSeatForecast item)
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

    public List<PendingLicenseSeatForecast> List()
    {
        var outValue = new List<PendingLicenseSeatForecast>();
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
public sealed class CachedPendingLicenseSeatForecastStore(MemoryPendingLicenseSeatForecastStore inner, PendingLicenseSeatForecastCache cache, int hits, int misses) : IPendingLicenseSeatForecastStore
{
    public MemoryPendingLicenseSeatForecastStore Inner { get; set; } = inner;
    public PendingLicenseSeatForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingLicenseSeatForecast? Find(string code)
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

    public void Save(PendingLicenseSeatForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingLicenseSeatForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending license seat forecasts. Empty fields match all.</summary>
public sealed record PendingLicenseSeatForecastQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PendingLicenseSeatForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingLicenseSeatForecast> Apply(List<PendingLicenseSeatForecast> items)
    {
        var matched = Collections.Filter(items, (PendingLicenseSeatForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending license seat forecast store.</summary>
public static class PendingLicenseSeatForecastStore
{
    public static IPendingLicenseSeatForecastStore NewMemoryPendingLicenseSeatForecastStore()
    {
        return new MemoryPendingLicenseSeatForecastStore(
            new Dictionary<string, PendingLicenseSeatForecast>(),
            new List<string>());
    }

    public static IPendingLicenseSeatForecastStore NewCachedPendingLicenseSeatForecastStore(MemoryPendingLicenseSeatForecastStore inner, int ttl, IClock clock)
    {
        return new CachedPendingLicenseSeatForecastStore(
            inner,
            PendingLicenseSeatForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingLicenseSeatForecast> FindPendingLicenseSeatForecasts(IPendingLicenseSeatForecastStore store, PendingLicenseSeatForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
