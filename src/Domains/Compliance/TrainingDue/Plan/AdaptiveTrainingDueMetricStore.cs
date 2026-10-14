namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive training due metrics by code.</summary>
public interface IAdaptiveTrainingDueMetricStore
{
    AdaptiveTrainingDueMetric? Find(string code);
    void Save(AdaptiveTrainingDueMetric item);
    bool Remove(string code);
    List<AdaptiveTrainingDueMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveTrainingDueMetricStore(Dictionary<string, AdaptiveTrainingDueMetric> items, List<string> order) : IAdaptiveTrainingDueMetricStore
{
    public Dictionary<string, AdaptiveTrainingDueMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveTrainingDueMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveTrainingDueMetric item)
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

    public List<AdaptiveTrainingDueMetric> List()
    {
        var outValue = new List<AdaptiveTrainingDueMetric>();
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
public sealed class CachedAdaptiveTrainingDueMetricStore(MemoryAdaptiveTrainingDueMetricStore inner, AdaptiveTrainingDueMetricCache cache, int hits, int misses) : IAdaptiveTrainingDueMetricStore
{
    public MemoryAdaptiveTrainingDueMetricStore Inner { get; set; } = inner;
    public AdaptiveTrainingDueMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveTrainingDueMetric? Find(string code)
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

    public void Save(AdaptiveTrainingDueMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveTrainingDueMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive training due metrics. Empty fields match all.</summary>
public sealed record AdaptiveTrainingDueMetricQuery(string Label, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveTrainingDueMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveTrainingDueMetric> Apply(List<AdaptiveTrainingDueMetric> items)
    {
        var matched = Collections.Filter(items, (AdaptiveTrainingDueMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive training due metric store.</summary>
public static class AdaptiveTrainingDueMetricStore
{
    public static IAdaptiveTrainingDueMetricStore NewMemoryAdaptiveTrainingDueMetricStore()
    {
        return new MemoryAdaptiveTrainingDueMetricStore(
            new Dictionary<string, AdaptiveTrainingDueMetric>(),
            new List<string>());
    }

    public static IAdaptiveTrainingDueMetricStore NewCachedAdaptiveTrainingDueMetricStore(MemoryAdaptiveTrainingDueMetricStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveTrainingDueMetricStore(
            inner,
            AdaptiveTrainingDueMetricCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveTrainingDueMetric> FindAdaptiveTrainingDueMetrics(IAdaptiveTrainingDueMetricStore store, AdaptiveTrainingDueMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
