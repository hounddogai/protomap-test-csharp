namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps sentiment forecasts by code.</summary>
public interface ISentimentForecastStore
{
    SentimentForecast? Find(string code);
    void Save(SentimentForecast item);
    bool Remove(string code);
    List<SentimentForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySentimentForecastStore(Dictionary<string, SentimentForecast> items, List<string> order) : ISentimentForecastStore
{
    public Dictionary<string, SentimentForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SentimentForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SentimentForecast item)
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

    public List<SentimentForecast> List()
    {
        var outValue = new List<SentimentForecast>();
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
public sealed class CachedSentimentForecastStore(MemorySentimentForecastStore inner, SentimentForecastCache cache, int hits, int misses) : ISentimentForecastStore
{
    public MemorySentimentForecastStore Inner { get; set; } = inner;
    public SentimentForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SentimentForecast? Find(string code)
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

    public void Save(SentimentForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SentimentForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects sentiment forecasts. Empty fields match all.</summary>
public sealed record SentimentForecastQuery(string Title, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SentimentForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SentimentForecast> Apply(List<SentimentForecast> items)
    {
        var matched = Collections.Filter(items, (SentimentForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of sentiment forecast store.</summary>
public static class SentimentForecastStore
{
    public static ISentimentForecastStore NewMemorySentimentForecastStore()
    {
        return new MemorySentimentForecastStore(new Dictionary<string, SentimentForecast>(), new List<string>());
    }

    public static ISentimentForecastStore NewCachedSentimentForecastStore(MemorySentimentForecastStore inner, int ttl, IClock clock)
    {
        return new CachedSentimentForecastStore(inner, SentimentForecastCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SentimentForecast> FindSentimentForecasts(ISentimentForecastStore store, SentimentForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
