namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request forecasts by code.</summary>
public interface ILeaveRequestForecastStore
{
    LeaveRequestForecast? Find(string code);
    void Save(LeaveRequestForecast item);
    bool Remove(string code);
    List<LeaveRequestForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestForecastStore(Dictionary<string, LeaveRequestForecast> items, List<string> order) : ILeaveRequestForecastStore
{
    public Dictionary<string, LeaveRequestForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestForecast item)
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

    public List<LeaveRequestForecast> List()
    {
        var outValue = new List<LeaveRequestForecast>();
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
public sealed class CachedLeaveRequestForecastStore(MemoryLeaveRequestForecastStore inner, LeaveRequestForecastCache cache, int hits, int misses) : ILeaveRequestForecastStore
{
    public MemoryLeaveRequestForecastStore Inner { get; set; } = inner;
    public LeaveRequestForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestForecast? Find(string code)
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

    public void Save(LeaveRequestForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request forecasts. Empty fields match all.</summary>
public sealed record LeaveRequestForecastQuery(string Source, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestForecast item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestForecast> Apply(List<LeaveRequestForecast> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request forecast store.</summary>
public static class LeaveRequestForecastStore
{
    public static ILeaveRequestForecastStore NewMemoryLeaveRequestForecastStore()
    {
        return new MemoryLeaveRequestForecastStore(
            new Dictionary<string, LeaveRequestForecast>(),
            new List<string>());
    }

    public static ILeaveRequestForecastStore NewCachedLeaveRequestForecastStore(MemoryLeaveRequestForecastStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestForecastStore(inner, LeaveRequestForecastCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestForecast> FindLeaveRequestForecasts(ILeaveRequestForecastStore store, LeaveRequestForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
