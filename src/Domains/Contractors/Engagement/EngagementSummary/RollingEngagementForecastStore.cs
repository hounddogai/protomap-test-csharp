namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling engagement forecasts by code.</summary>
public interface IRollingEngagementForecastStore
{
    RollingEngagementForecast? Find(string code);
    void Save(RollingEngagementForecast item);
    bool Remove(string code);
    List<RollingEngagementForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingEngagementForecastStore(Dictionary<string, RollingEngagementForecast> items, List<string> order) : IRollingEngagementForecastStore
{
    public Dictionary<string, RollingEngagementForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingEngagementForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingEngagementForecast item)
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

    public List<RollingEngagementForecast> List()
    {
        var outValue = new List<RollingEngagementForecast>();
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
public sealed class CachedRollingEngagementForecastStore(MemoryRollingEngagementForecastStore inner, RollingEngagementForecastCache cache, int hits, int misses) : IRollingEngagementForecastStore
{
    public MemoryRollingEngagementForecastStore Inner { get; set; } = inner;
    public RollingEngagementForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingEngagementForecast? Find(string code)
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

    public void Save(RollingEngagementForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingEngagementForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling engagement forecasts. Empty fields match all.</summary>
public sealed record RollingEngagementForecastQuery(string Locale, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(RollingEngagementForecast item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<RollingEngagementForecast> Apply(List<RollingEngagementForecast> items)
    {
        var matched = Collections.Filter(items, (RollingEngagementForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling engagement forecast store.</summary>
public static class RollingEngagementForecastStore
{
    public static IRollingEngagementForecastStore NewMemoryRollingEngagementForecastStore()
    {
        return new MemoryRollingEngagementForecastStore(
            new Dictionary<string, RollingEngagementForecast>(),
            new List<string>());
    }

    public static IRollingEngagementForecastStore NewCachedRollingEngagementForecastStore(MemoryRollingEngagementForecastStore inner, int ttl, IClock clock)
    {
        return new CachedRollingEngagementForecastStore(
            inner,
            RollingEngagementForecastCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RollingEngagementForecast> FindRollingEngagementForecasts(IRollingEngagementForecastStore store, RollingEngagementForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
