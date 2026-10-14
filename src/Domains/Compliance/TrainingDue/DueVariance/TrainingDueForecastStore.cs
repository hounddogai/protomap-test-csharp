namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due forecasts by code.</summary>
public interface ITrainingDueForecastStore
{
    TrainingDueForecast? Find(string code);
    void Save(TrainingDueForecast item);
    bool Remove(string code);
    List<TrainingDueForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueForecastStore(Dictionary<string, TrainingDueForecast> items, List<string> order) : ITrainingDueForecastStore
{
    public Dictionary<string, TrainingDueForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueForecast item)
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

    public List<TrainingDueForecast> List()
    {
        var outValue = new List<TrainingDueForecast>();
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
public sealed class CachedTrainingDueForecastStore(MemoryTrainingDueForecastStore inner, TrainingDueForecastCache cache, int hits, int misses) : ITrainingDueForecastStore
{
    public MemoryTrainingDueForecastStore Inner { get; set; } = inner;
    public TrainingDueForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueForecast? Find(string code)
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

    public void Save(TrainingDueForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due forecasts. Empty fields match all.</summary>
public sealed record TrainingDueForecastQuery(string Label, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueForecast> Apply(List<TrainingDueForecast> items)
    {
        var matched = Collections.Filter(items, (TrainingDueForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due forecast store.</summary>
public static class TrainingDueForecastStore
{
    public static ITrainingDueForecastStore NewMemoryTrainingDueForecastStore()
    {
        return new MemoryTrainingDueForecastStore(new Dictionary<string, TrainingDueForecast>(), new List<string>());
    }

    public static ITrainingDueForecastStore NewCachedTrainingDueForecastStore(MemoryTrainingDueForecastStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueForecastStore(inner, TrainingDueForecastCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TrainingDueForecast> FindTrainingDueForecasts(ITrainingDueForecastStore store, TrainingDueForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
