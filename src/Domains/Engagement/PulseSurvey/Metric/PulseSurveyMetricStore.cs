namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey metrics by code.</summary>
public interface IPulseSurveyMetricStore
{
    PulseSurveyMetric? Find(string code);
    void Save(PulseSurveyMetric item);
    bool Remove(string code);
    List<PulseSurveyMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveyMetricStore(Dictionary<string, PulseSurveyMetric> items, List<string> order) : IPulseSurveyMetricStore
{
    public Dictionary<string, PulseSurveyMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveyMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveyMetric item)
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

    public List<PulseSurveyMetric> List()
    {
        var outValue = new List<PulseSurveyMetric>();
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
public sealed class CachedPulseSurveyMetricStore(MemoryPulseSurveyMetricStore inner, PulseSurveyMetricCache cache, int hits, int misses) : IPulseSurveyMetricStore
{
    public MemoryPulseSurveyMetricStore Inner { get; set; } = inner;
    public PulseSurveyMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveyMetric? Find(string code)
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

    public void Save(PulseSurveyMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveyMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey metrics. Empty fields match all.</summary>
public sealed record PulseSurveyMetricQuery(string Department, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveyMetric item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveyMetric> Apply(List<PulseSurveyMetric> items)
    {
        var matched = Collections.Filter(items, (PulseSurveyMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey metric store.</summary>
public static class PulseSurveyMetricStore
{
    public static IPulseSurveyMetricStore NewMemoryPulseSurveyMetricStore()
    {
        return new MemoryPulseSurveyMetricStore(new Dictionary<string, PulseSurveyMetric>(), new List<string>());
    }

    public static IPulseSurveyMetricStore NewCachedPulseSurveyMetricStore(MemoryPulseSurveyMetricStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveyMetricStore(inner, PulseSurveyMetricCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PulseSurveyMetric> FindPulseSurveyMetrics(IPulseSurveyMetricStore store, PulseSurveyMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
