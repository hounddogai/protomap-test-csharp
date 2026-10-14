namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict pulse survey variances by code.</summary>
public interface IStrictPulseSurveyVarianceStore
{
    StrictPulseSurveyVariance? Find(string code);
    void Save(StrictPulseSurveyVariance item);
    bool Remove(string code);
    List<StrictPulseSurveyVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPulseSurveyVarianceStore(Dictionary<string, StrictPulseSurveyVariance> items, List<string> order) : IStrictPulseSurveyVarianceStore
{
    public Dictionary<string, StrictPulseSurveyVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPulseSurveyVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPulseSurveyVariance item)
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

    public List<StrictPulseSurveyVariance> List()
    {
        var outValue = new List<StrictPulseSurveyVariance>();
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
public sealed class CachedStrictPulseSurveyVarianceStore(MemoryStrictPulseSurveyVarianceStore inner, StrictPulseSurveyVarianceCache cache, int hits, int misses) : IStrictPulseSurveyVarianceStore
{
    public MemoryStrictPulseSurveyVarianceStore Inner { get; set; } = inner;
    public StrictPulseSurveyVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPulseSurveyVariance? Find(string code)
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

    public void Save(StrictPulseSurveyVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPulseSurveyVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict pulse survey variances. Empty fields match all.</summary>
public sealed record StrictPulseSurveyVarianceQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPulseSurveyVariance item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPulseSurveyVariance> Apply(List<StrictPulseSurveyVariance> items)
    {
        var matched = Collections.Filter(items, (StrictPulseSurveyVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict pulse survey variance store.</summary>
public static class StrictPulseSurveyVarianceStore
{
    public static IStrictPulseSurveyVarianceStore NewMemoryStrictPulseSurveyVarianceStore()
    {
        return new MemoryStrictPulseSurveyVarianceStore(
            new Dictionary<string, StrictPulseSurveyVariance>(),
            new List<string>());
    }

    public static IStrictPulseSurveyVarianceStore NewCachedStrictPulseSurveyVarianceStore(MemoryStrictPulseSurveyVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPulseSurveyVarianceStore(
            inner,
            StrictPulseSurveyVarianceCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictPulseSurveyVariance> FindStrictPulseSurveyVariances(IStrictPulseSurveyVarianceStore store, StrictPulseSurveyVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
