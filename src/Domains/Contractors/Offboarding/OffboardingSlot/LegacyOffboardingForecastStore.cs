namespace Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy offboarding forecasts by code.</summary>
public interface ILegacyOffboardingForecastStore
{
    LegacyOffboardingForecast? Find(string code);
    void Save(LegacyOffboardingForecast item);
    bool Remove(string code);
    List<LegacyOffboardingForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyOffboardingForecastStore(Dictionary<string, LegacyOffboardingForecast> items, List<string> order) : ILegacyOffboardingForecastStore
{
    public Dictionary<string, LegacyOffboardingForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyOffboardingForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyOffboardingForecast item)
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

    public List<LegacyOffboardingForecast> List()
    {
        var outValue = new List<LegacyOffboardingForecast>();
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
public sealed class CachedLegacyOffboardingForecastStore(MemoryLegacyOffboardingForecastStore inner, LegacyOffboardingForecastCache cache, int hits, int misses) : ILegacyOffboardingForecastStore
{
    public MemoryLegacyOffboardingForecastStore Inner { get; set; } = inner;
    public LegacyOffboardingForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyOffboardingForecast? Find(string code)
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

    public void Save(LegacyOffboardingForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyOffboardingForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy offboarding forecasts. Empty fields match all.</summary>
public sealed record LegacyOffboardingForecastQuery(string Channel, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyOffboardingForecast item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyOffboardingForecast> Apply(List<LegacyOffboardingForecast> items)
    {
        var matched = Collections.Filter(items, (LegacyOffboardingForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy offboarding forecast store.</summary>
public static class LegacyOffboardingForecastStore
{
    public static ILegacyOffboardingForecastStore NewMemoryLegacyOffboardingForecastStore()
    {
        return new MemoryLegacyOffboardingForecastStore(
            new Dictionary<string, LegacyOffboardingForecast>(),
            new List<string>());
    }

    public static ILegacyOffboardingForecastStore NewCachedLegacyOffboardingForecastStore(MemoryLegacyOffboardingForecastStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyOffboardingForecastStore(
            inner,
            LegacyOffboardingForecastCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyOffboardingForecast> FindLegacyOffboardingForecasts(ILegacyOffboardingForecastStore store, LegacyOffboardingForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
