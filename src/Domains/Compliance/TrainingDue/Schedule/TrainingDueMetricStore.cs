namespace Acme.Workforce.Compliance.TrainingDue.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due metrics by code.</summary>
public interface ITrainingDueMetricStore
{
    TrainingDueMetric? Find(string code);
    void Save(TrainingDueMetric item);
    bool Remove(string code);
    List<TrainingDueMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueMetricStore(Dictionary<string, TrainingDueMetric> items, List<string> order) : ITrainingDueMetricStore
{
    public Dictionary<string, TrainingDueMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueMetric item)
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

    public List<TrainingDueMetric> List()
    {
        var outValue = new List<TrainingDueMetric>();
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
public sealed class CachedTrainingDueMetricStore(MemoryTrainingDueMetricStore inner, TrainingDueMetricCache cache, int hits, int misses) : ITrainingDueMetricStore
{
    public MemoryTrainingDueMetricStore Inner { get; set; } = inner;
    public TrainingDueMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueMetric? Find(string code)
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

    public void Save(TrainingDueMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due metrics. Empty fields match all.</summary>
public sealed record TrainingDueMetricQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueMetric item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<TrainingDueMetric> Apply(List<TrainingDueMetric> items)
    {
        var matched = Collections.Filter(items, (TrainingDueMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due metric store.</summary>
public static class TrainingDueMetricStore
{
    public static ITrainingDueMetricStore NewMemoryTrainingDueMetricStore()
    {
        return new MemoryTrainingDueMetricStore(new Dictionary<string, TrainingDueMetric>(), new List<string>());
    }

    public static ITrainingDueMetricStore NewCachedTrainingDueMetricStore(MemoryTrainingDueMetricStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueMetricStore(inner, TrainingDueMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TrainingDueMetric> FindTrainingDueMetrics(ITrainingDueMetricStore store, TrainingDueMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
