namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive feedback forecasts by code.</summary>
public interface IAdaptiveFeedbackForecastStore
{
    AdaptiveFeedbackForecast? Find(string code);
    void Save(AdaptiveFeedbackForecast item);
    bool Remove(string code);
    List<AdaptiveFeedbackForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveFeedbackForecastStore(Dictionary<string, AdaptiveFeedbackForecast> items, List<string> order) : IAdaptiveFeedbackForecastStore
{
    public Dictionary<string, AdaptiveFeedbackForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveFeedbackForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveFeedbackForecast item)
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

    public List<AdaptiveFeedbackForecast> List()
    {
        var outValue = new List<AdaptiveFeedbackForecast>();
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
public sealed class CachedAdaptiveFeedbackForecastStore(MemoryAdaptiveFeedbackForecastStore inner, AdaptiveFeedbackForecastCache cache, int hits, int misses) : IAdaptiveFeedbackForecastStore
{
    public MemoryAdaptiveFeedbackForecastStore Inner { get; set; } = inner;
    public AdaptiveFeedbackForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveFeedbackForecast? Find(string code)
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

    public void Save(AdaptiveFeedbackForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveFeedbackForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive feedback forecasts. Empty fields match all.</summary>
public sealed record AdaptiveFeedbackForecastQuery(string Title, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveFeedbackForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<AdaptiveFeedbackForecast> Apply(List<AdaptiveFeedbackForecast> items)
    {
        var matched = Collections.Filter(items, (AdaptiveFeedbackForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive feedback forecast store.</summary>
public static class AdaptiveFeedbackForecastStore
{
    public static IAdaptiveFeedbackForecastStore NewMemoryAdaptiveFeedbackForecastStore()
    {
        return new MemoryAdaptiveFeedbackForecastStore(
            new Dictionary<string, AdaptiveFeedbackForecast>(),
            new List<string>());
    }

    public static IAdaptiveFeedbackForecastStore NewCachedAdaptiveFeedbackForecastStore(MemoryAdaptiveFeedbackForecastStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveFeedbackForecastStore(
            inner,
            AdaptiveFeedbackForecastCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveFeedbackForecast> FindAdaptiveFeedbackForecasts(IAdaptiveFeedbackForecastStore store, AdaptiveFeedbackForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
