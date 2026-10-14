namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual pulse survey forecasts by code.</summary>
public interface IManualPulseSurveyForecastStore
{
    ManualPulseSurveyForecast? Find(string code);
    void Save(ManualPulseSurveyForecast item);
    bool Remove(string code);
    List<ManualPulseSurveyForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPulseSurveyForecastStore(Dictionary<string, ManualPulseSurveyForecast> items, List<string> order) : IManualPulseSurveyForecastStore
{
    public Dictionary<string, ManualPulseSurveyForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPulseSurveyForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPulseSurveyForecast item)
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

    public List<ManualPulseSurveyForecast> List()
    {
        var outValue = new List<ManualPulseSurveyForecast>();
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
public sealed class CachedManualPulseSurveyForecastStore(MemoryManualPulseSurveyForecastStore inner, ManualPulseSurveyForecastCache cache, int hits, int misses) : IManualPulseSurveyForecastStore
{
    public MemoryManualPulseSurveyForecastStore Inner { get; set; } = inner;
    public ManualPulseSurveyForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPulseSurveyForecast? Find(string code)
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

    public void Save(ManualPulseSurveyForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPulseSurveyForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual pulse survey forecasts. Empty fields match all.</summary>
public sealed record ManualPulseSurveyForecastQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPulseSurveyForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualPulseSurveyForecast> Apply(List<ManualPulseSurveyForecast> items)
    {
        var matched = Collections.Filter(items, (ManualPulseSurveyForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual pulse survey forecast store.</summary>
public static class ManualPulseSurveyForecastStore
{
    public static IManualPulseSurveyForecastStore NewMemoryManualPulseSurveyForecastStore()
    {
        return new MemoryManualPulseSurveyForecastStore(
            new Dictionary<string, ManualPulseSurveyForecast>(),
            new List<string>());
    }

    public static IManualPulseSurveyForecastStore NewCachedManualPulseSurveyForecastStore(MemoryManualPulseSurveyForecastStore inner, int ttl, IClock clock)
    {
        return new CachedManualPulseSurveyForecastStore(
            inner,
            ManualPulseSurveyForecastCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ManualPulseSurveyForecast> FindManualPulseSurveyForecasts(IManualPulseSurveyForecastStore store, ManualPulseSurveyForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
