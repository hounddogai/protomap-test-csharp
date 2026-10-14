namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk forecasts by code.</summary>
public interface IDeskForecastStore
{
    DeskForecast? Find(string code);
    void Save(DeskForecast item);
    bool Remove(string code);
    List<DeskForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskForecastStore(Dictionary<string, DeskForecast> items, List<string> order) : IDeskForecastStore
{
    public Dictionary<string, DeskForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskForecast item)
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

    public List<DeskForecast> List()
    {
        var outValue = new List<DeskForecast>();
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
public sealed class CachedDeskForecastStore(MemoryDeskForecastStore inner, DeskForecastCache cache, int hits, int misses) : IDeskForecastStore
{
    public MemoryDeskForecastStore Inner { get; set; } = inner;
    public DeskForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskForecast? Find(string code)
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

    public void Save(DeskForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk forecasts. Empty fields match all.</summary>
public sealed record DeskForecastQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(DeskForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskForecast> Apply(List<DeskForecast> items)
    {
        var matched = Collections.Filter(items, (DeskForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk forecast store.</summary>
public static class DeskForecastStore
{
    public static IDeskForecastStore NewMemoryDeskForecastStore()
    {
        return new MemoryDeskForecastStore(new Dictionary<string, DeskForecast>(), new List<string>());
    }

    public static IDeskForecastStore NewCachedDeskForecastStore(MemoryDeskForecastStore inner, int ttl, IClock clock)
    {
        return new CachedDeskForecastStore(inner, DeskForecastCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeskForecast> FindDeskForecasts(IDeskForecastStore store, DeskForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
