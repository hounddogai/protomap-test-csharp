namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient leave request forecasts by code.</summary>
public interface ILenientLeaveRequestForecastStore
{
    LenientLeaveRequestForecast? Find(string code);
    void Save(LenientLeaveRequestForecast item);
    bool Remove(string code);
    List<LenientLeaveRequestForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientLeaveRequestForecastStore(Dictionary<string, LenientLeaveRequestForecast> items, List<string> order) : ILenientLeaveRequestForecastStore
{
    public Dictionary<string, LenientLeaveRequestForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientLeaveRequestForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientLeaveRequestForecast item)
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

    public List<LenientLeaveRequestForecast> List()
    {
        var outValue = new List<LenientLeaveRequestForecast>();
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
public sealed class CachedLenientLeaveRequestForecastStore(MemoryLenientLeaveRequestForecastStore inner, LenientLeaveRequestForecastCache cache, int hits, int misses) : ILenientLeaveRequestForecastStore
{
    public MemoryLenientLeaveRequestForecastStore Inner { get; set; } = inner;
    public LenientLeaveRequestForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientLeaveRequestForecast? Find(string code)
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

    public void Save(LenientLeaveRequestForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientLeaveRequestForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient leave request forecasts. Empty fields match all.</summary>
public sealed record LenientLeaveRequestForecastQuery(string Category, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(LenientLeaveRequestForecast item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientLeaveRequestForecast> Apply(List<LenientLeaveRequestForecast> items)
    {
        var matched = Collections.Filter(items, (LenientLeaveRequestForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient leave request forecast store.</summary>
public static class LenientLeaveRequestForecastStore
{
    public static ILenientLeaveRequestForecastStore NewMemoryLenientLeaveRequestForecastStore()
    {
        return new MemoryLenientLeaveRequestForecastStore(
            new Dictionary<string, LenientLeaveRequestForecast>(),
            new List<string>());
    }

    public static ILenientLeaveRequestForecastStore NewCachedLenientLeaveRequestForecastStore(MemoryLenientLeaveRequestForecastStore inner, int ttl, IClock clock)
    {
        return new CachedLenientLeaveRequestForecastStore(
            inner,
            LenientLeaveRequestForecastCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientLeaveRequestForecast> FindLenientLeaveRequestForecasts(ILenientLeaveRequestForecastStore store, LenientLeaveRequestForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
