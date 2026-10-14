namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet forecasts by code.</summary>
public interface ITimesheetForecastStore
{
    TimesheetForecast? Find(string code);
    void Save(TimesheetForecast item);
    bool Remove(string code);
    List<TimesheetForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetForecastStore(Dictionary<string, TimesheetForecast> items, List<string> order) : ITimesheetForecastStore
{
    public Dictionary<string, TimesheetForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetForecast item)
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

    public List<TimesheetForecast> List()
    {
        var outValue = new List<TimesheetForecast>();
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
public sealed class CachedTimesheetForecastStore(MemoryTimesheetForecastStore inner, TimesheetForecastCache cache, int hits, int misses) : ITimesheetForecastStore
{
    public MemoryTimesheetForecastStore Inner { get; set; } = inner;
    public TimesheetForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetForecast? Find(string code)
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

    public void Save(TimesheetForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet forecasts. Empty fields match all.</summary>
public sealed record TimesheetForecastQuery(string Name, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetForecast item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetForecast> Apply(List<TimesheetForecast> items)
    {
        var matched = Collections.Filter(items, (TimesheetForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet forecast store.</summary>
public static class TimesheetForecastStore
{
    public static ITimesheetForecastStore NewMemoryTimesheetForecastStore()
    {
        return new MemoryTimesheetForecastStore(new Dictionary<string, TimesheetForecast>(), new List<string>());
    }

    public static ITimesheetForecastStore NewCachedTimesheetForecastStore(MemoryTimesheetForecastStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetForecastStore(inner, TimesheetForecastCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TimesheetForecast> FindTimesheetForecasts(ITimesheetForecastStore store, TimesheetForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
