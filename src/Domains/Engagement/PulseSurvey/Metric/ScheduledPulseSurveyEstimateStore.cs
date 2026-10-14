namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled pulse survey estimates by code.</summary>
public interface IScheduledPulseSurveyEstimateStore
{
    ScheduledPulseSurveyEstimate? Find(string code);
    void Save(ScheduledPulseSurveyEstimate item);
    bool Remove(string code);
    List<ScheduledPulseSurveyEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPulseSurveyEstimateStore(Dictionary<string, ScheduledPulseSurveyEstimate> items, List<string> order) : IScheduledPulseSurveyEstimateStore
{
    public Dictionary<string, ScheduledPulseSurveyEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPulseSurveyEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPulseSurveyEstimate item)
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

    public List<ScheduledPulseSurveyEstimate> List()
    {
        var outValue = new List<ScheduledPulseSurveyEstimate>();
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
public sealed class CachedScheduledPulseSurveyEstimateStore(MemoryScheduledPulseSurveyEstimateStore inner, ScheduledPulseSurveyEstimateCache cache, int hits, int misses) : IScheduledPulseSurveyEstimateStore
{
    public MemoryScheduledPulseSurveyEstimateStore Inner { get; set; } = inner;
    public ScheduledPulseSurveyEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPulseSurveyEstimate? Find(string code)
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

    public void Save(ScheduledPulseSurveyEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPulseSurveyEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled pulse survey estimates. Empty fields match all.</summary>
public sealed record ScheduledPulseSurveyEstimateQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPulseSurveyEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledPulseSurveyEstimate> Apply(List<ScheduledPulseSurveyEstimate> items)
    {
        var matched = Collections.Filter(items, (ScheduledPulseSurveyEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled pulse survey estimate store.</summary>
public static class ScheduledPulseSurveyEstimateStore
{
    public static IScheduledPulseSurveyEstimateStore NewMemoryScheduledPulseSurveyEstimateStore()
    {
        return new MemoryScheduledPulseSurveyEstimateStore(
            new Dictionary<string, ScheduledPulseSurveyEstimate>(),
            new List<string>());
    }

    public static IScheduledPulseSurveyEstimateStore NewCachedScheduledPulseSurveyEstimateStore(MemoryScheduledPulseSurveyEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPulseSurveyEstimateStore(
            inner,
            ScheduledPulseSurveyEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledPulseSurveyEstimate> FindScheduledPulseSurveyEstimates(IScheduledPulseSurveyEstimateStore store, ScheduledPulseSurveyEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
