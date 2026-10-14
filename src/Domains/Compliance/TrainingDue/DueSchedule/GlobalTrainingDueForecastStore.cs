namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global training due forecasts by code.</summary>
public interface IGlobalTrainingDueForecastStore
{
    GlobalTrainingDueForecast? Find(string code);
    void Save(GlobalTrainingDueForecast item);
    bool Remove(string code);
    List<GlobalTrainingDueForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalTrainingDueForecastStore(Dictionary<string, GlobalTrainingDueForecast> items, List<string> order) : IGlobalTrainingDueForecastStore
{
    public Dictionary<string, GlobalTrainingDueForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalTrainingDueForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalTrainingDueForecast item)
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

    public List<GlobalTrainingDueForecast> List()
    {
        var outValue = new List<GlobalTrainingDueForecast>();
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
public sealed class CachedGlobalTrainingDueForecastStore(MemoryGlobalTrainingDueForecastStore inner, GlobalTrainingDueForecastCache cache, int hits, int misses) : IGlobalTrainingDueForecastStore
{
    public MemoryGlobalTrainingDueForecastStore Inner { get; set; } = inner;
    public GlobalTrainingDueForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalTrainingDueForecast? Find(string code)
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

    public void Save(GlobalTrainingDueForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalTrainingDueForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global training due forecasts. Empty fields match all.</summary>
public sealed record GlobalTrainingDueForecastQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalTrainingDueForecast item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalTrainingDueForecast> Apply(List<GlobalTrainingDueForecast> items)
    {
        var matched = Collections.Filter(items, (GlobalTrainingDueForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global training due forecast store.</summary>
public static class GlobalTrainingDueForecastStore
{
    public static IGlobalTrainingDueForecastStore NewMemoryGlobalTrainingDueForecastStore()
    {
        return new MemoryGlobalTrainingDueForecastStore(
            new Dictionary<string, GlobalTrainingDueForecast>(),
            new List<string>());
    }

    public static IGlobalTrainingDueForecastStore NewCachedGlobalTrainingDueForecastStore(MemoryGlobalTrainingDueForecastStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalTrainingDueForecastStore(
            inner,
            GlobalTrainingDueForecastCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<GlobalTrainingDueForecast> FindGlobalTrainingDueForecasts(IGlobalTrainingDueForecastStore store, GlobalTrainingDueForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
