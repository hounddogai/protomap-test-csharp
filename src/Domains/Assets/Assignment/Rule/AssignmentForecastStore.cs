namespace Acme.Workforce.Assets.Assignment.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment forecasts by code.</summary>
public interface IAssignmentForecastStore
{
    AssignmentForecast? Find(string code);
    void Save(AssignmentForecast item);
    bool Remove(string code);
    List<AssignmentForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentForecastStore(Dictionary<string, AssignmentForecast> items, List<string> order) : IAssignmentForecastStore
{
    public Dictionary<string, AssignmentForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentForecast item)
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

    public List<AssignmentForecast> List()
    {
        var outValue = new List<AssignmentForecast>();
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
public sealed class CachedAssignmentForecastStore(MemoryAssignmentForecastStore inner, AssignmentForecastCache cache, int hits, int misses) : IAssignmentForecastStore
{
    public MemoryAssignmentForecastStore Inner { get; set; } = inner;
    public AssignmentForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentForecast? Find(string code)
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

    public void Save(AssignmentForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment forecasts. Empty fields match all.</summary>
public sealed record AssignmentForecastQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentForecast> Apply(List<AssignmentForecast> items)
    {
        var matched = Collections.Filter(items, (AssignmentForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment forecast store.</summary>
public static class AssignmentForecastStore
{
    public static IAssignmentForecastStore NewMemoryAssignmentForecastStore()
    {
        return new MemoryAssignmentForecastStore(new Dictionary<string, AssignmentForecast>(), new List<string>());
    }

    public static IAssignmentForecastStore NewCachedAssignmentForecastStore(MemoryAssignmentForecastStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentForecastStore(inner, AssignmentForecastCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssignmentForecast> FindAssignmentForecasts(IAssignmentForecastStore store, AssignmentForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
