namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled feedback forecasts by code.</summary>
public interface IScheduledFeedbackForecastStore
{
    ScheduledFeedbackForecast? Find(string code);
    void Save(ScheduledFeedbackForecast item);
    bool Remove(string code);
    List<ScheduledFeedbackForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledFeedbackForecastStore(Dictionary<string, ScheduledFeedbackForecast> items, List<string> order) : IScheduledFeedbackForecastStore
{
    public Dictionary<string, ScheduledFeedbackForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledFeedbackForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledFeedbackForecast item)
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

    public List<ScheduledFeedbackForecast> List()
    {
        var outValue = new List<ScheduledFeedbackForecast>();
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
public sealed class CachedScheduledFeedbackForecastStore(MemoryScheduledFeedbackForecastStore inner, ScheduledFeedbackForecastCache cache, int hits, int misses) : IScheduledFeedbackForecastStore
{
    public MemoryScheduledFeedbackForecastStore Inner { get; set; } = inner;
    public ScheduledFeedbackForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledFeedbackForecast? Find(string code)
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

    public void Save(ScheduledFeedbackForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledFeedbackForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled feedback forecasts. Empty fields match all.</summary>
public sealed record ScheduledFeedbackForecastQuery(string Locale, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledFeedbackForecast item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledFeedbackForecast> Apply(List<ScheduledFeedbackForecast> items)
    {
        var matched = Collections.Filter(items, (ScheduledFeedbackForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled feedback forecast store.</summary>
public static class ScheduledFeedbackForecastStore
{
    public static IScheduledFeedbackForecastStore NewMemoryScheduledFeedbackForecastStore()
    {
        return new MemoryScheduledFeedbackForecastStore(
            new Dictionary<string, ScheduledFeedbackForecast>(),
            new List<string>());
    }

    public static IScheduledFeedbackForecastStore NewCachedScheduledFeedbackForecastStore(MemoryScheduledFeedbackForecastStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledFeedbackForecastStore(
            inner,
            ScheduledFeedbackForecastCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledFeedbackForecast> FindScheduledFeedbackForecasts(IScheduledFeedbackForecastStore store, ScheduledFeedbackForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
