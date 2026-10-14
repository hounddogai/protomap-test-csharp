namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey forecasts by code.</summary>
public interface IPulseSurveyForecastStore
{
    PulseSurveyForecast? Find(string code);
    void Save(PulseSurveyForecast item);
    bool Remove(string code);
    List<PulseSurveyForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyForecastStore(Dictionary<string, PulseSurveyForecast> items, List<string> order) : IPulseSurveyForecastStore
{
    public Dictionary<string, PulseSurveyForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyForecast item)
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

    public List<PulseSurveyForecast> List()
    {
        var outValue = new List<PulseSurveyForecast>();
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
public sealed class CachedPulseSurveyForecastStore(MemoryPulseSurveyForecastStore inner, PulseSurveyForecastCache cache, int hits, int misses) : IPulseSurveyForecastStore
{
    public MemoryPulseSurveyForecastStore Inner { get; set; } = inner;
    public PulseSurveyForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyForecast? Find(string code)
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

    public void Save(PulseSurveyForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey forecasts. Empty fields match all.</summary>
public sealed record PulseSurveyForecastQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyForecast item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyForecast> Apply(List<PulseSurveyForecast> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey forecast store.</summary>
public static class PulseSurveyForecastStore
{
    public static IPulseSurveyForecastStore NewMemoryPulseSurveyForecastStore()
    {
        return new MemoryPulseSurveyForecastStore(new Dictionary<string, PulseSurveyForecast>(), new List<string>());
    }

    public static IPulseSurveyForecastStore NewCachedPulseSurveyForecastStore(MemoryPulseSurveyForecastStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyForecastStore(inner, PulseSurveyForecastCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyForecast> FindPulseSurveyForecasts(IPulseSurveyForecastStore store, PulseSurveyForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
