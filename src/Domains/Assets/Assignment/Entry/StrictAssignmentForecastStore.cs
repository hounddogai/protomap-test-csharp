namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict assignment forecasts by code.</summary>
public interface IStrictAssignmentForecastStore
{
    StrictAssignmentForecast? Find(string code);
    void Save(StrictAssignmentForecast item);
    bool Remove(string code);
    List<StrictAssignmentForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAssignmentForecastStore(Dictionary<string, StrictAssignmentForecast> items, List<string> order) : IStrictAssignmentForecastStore
{
    public Dictionary<string, StrictAssignmentForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAssignmentForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAssignmentForecast item)
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

    public List<StrictAssignmentForecast> List()
    {
        var outValue = new List<StrictAssignmentForecast>();
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
public sealed class CachedStrictAssignmentForecastStore(MemoryStrictAssignmentForecastStore inner, StrictAssignmentForecastCache cache, int hits, int misses) : IStrictAssignmentForecastStore
{
    public MemoryStrictAssignmentForecastStore Inner { get; set; } = inner;
    public StrictAssignmentForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAssignmentForecast? Find(string code)
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

    public void Save(StrictAssignmentForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAssignmentForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict assignment forecasts. Empty fields match all.</summary>
public sealed record StrictAssignmentForecastQuery(string Name, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAssignmentForecast item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAssignmentForecast> Apply(List<StrictAssignmentForecast> items)
    {
        var matched = Collections.Filter(items, (StrictAssignmentForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict assignment forecast store.</summary>
public static class StrictAssignmentForecastStore
{
    public static IStrictAssignmentForecastStore NewMemoryStrictAssignmentForecastStore()
    {
        return new MemoryStrictAssignmentForecastStore(
            new Dictionary<string, StrictAssignmentForecast>(),
            new List<string>());
    }

    public static IStrictAssignmentForecastStore NewCachedStrictAssignmentForecastStore(MemoryStrictAssignmentForecastStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAssignmentForecastStore(
            inner,
            StrictAssignmentForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StrictAssignmentForecast> FindStrictAssignmentForecasts(IStrictAssignmentForecastStore store, StrictAssignmentForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
