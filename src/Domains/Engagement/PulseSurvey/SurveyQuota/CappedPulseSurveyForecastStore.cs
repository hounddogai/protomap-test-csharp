namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped pulse survey forecasts by code.</summary>
public interface ICappedPulseSurveyForecastStore
{
    CappedPulseSurveyForecast? Find(string code);
    void Save(CappedPulseSurveyForecast item);
    bool Remove(string code);
    List<CappedPulseSurveyForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPulseSurveyForecastStore(Dictionary<string, CappedPulseSurveyForecast> items, List<string> order) : ICappedPulseSurveyForecastStore
{
    public Dictionary<string, CappedPulseSurveyForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPulseSurveyForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPulseSurveyForecast item)
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

    public List<CappedPulseSurveyForecast> List()
    {
        var outValue = new List<CappedPulseSurveyForecast>();
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
public sealed class CachedCappedPulseSurveyForecastStore(MemoryCappedPulseSurveyForecastStore inner, CappedPulseSurveyForecastCache cache, int hits, int misses) : ICappedPulseSurveyForecastStore
{
    public MemoryCappedPulseSurveyForecastStore Inner { get; set; } = inner;
    public CappedPulseSurveyForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPulseSurveyForecast? Find(string code)
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

    public void Save(CappedPulseSurveyForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPulseSurveyForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped pulse survey forecasts. Empty fields match all.</summary>
public sealed record CappedPulseSurveyForecastQuery(string Title, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPulseSurveyForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<CappedPulseSurveyForecast> Apply(List<CappedPulseSurveyForecast> items)
    {
        var matched = Collections.Filter(items, (CappedPulseSurveyForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped pulse survey forecast store.</summary>
public static class CappedPulseSurveyForecastStore
{
    public static ICappedPulseSurveyForecastStore NewMemoryCappedPulseSurveyForecastStore()
    {
        return new MemoryCappedPulseSurveyForecastStore(
            new Dictionary<string, CappedPulseSurveyForecast>(),
            new List<string>());
    }

    public static ICappedPulseSurveyForecastStore NewCachedCappedPulseSurveyForecastStore(MemoryCappedPulseSurveyForecastStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPulseSurveyForecastStore(
            inner,
            CappedPulseSurveyForecastCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedPulseSurveyForecast> FindCappedPulseSurveyForecasts(ICappedPulseSurveyForecastStore store, CappedPulseSurveyForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
