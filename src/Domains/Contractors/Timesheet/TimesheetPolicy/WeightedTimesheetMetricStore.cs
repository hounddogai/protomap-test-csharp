namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted timesheet metrics by code.</summary>
public interface IWeightedTimesheetMetricStore
{
    WeightedTimesheetMetric? Find(string code);
    void Save(WeightedTimesheetMetric item);
    bool Remove(string code);
    List<WeightedTimesheetMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedTimesheetMetricStore(Dictionary<string, WeightedTimesheetMetric> items, List<string> order) : IWeightedTimesheetMetricStore
{
    public Dictionary<string, WeightedTimesheetMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedTimesheetMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedTimesheetMetric item)
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

    public List<WeightedTimesheetMetric> List()
    {
        var outValue = new List<WeightedTimesheetMetric>();
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
public sealed class CachedWeightedTimesheetMetricStore(MemoryWeightedTimesheetMetricStore inner, WeightedTimesheetMetricCache cache, int hits, int misses) : IWeightedTimesheetMetricStore
{
    public MemoryWeightedTimesheetMetricStore Inner { get; set; } = inner;
    public WeightedTimesheetMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedTimesheetMetric? Find(string code)
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

    public void Save(WeightedTimesheetMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedTimesheetMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted timesheet metrics. Empty fields match all.</summary>
public sealed record WeightedTimesheetMetricQuery(string Region, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedTimesheetMetric item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedTimesheetMetric> Apply(List<WeightedTimesheetMetric> items)
    {
        var matched = Collections.Filter(items, (WeightedTimesheetMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted timesheet metric store.</summary>
public static class WeightedTimesheetMetricStore
{
    public static IWeightedTimesheetMetricStore NewMemoryWeightedTimesheetMetricStore()
    {
        return new MemoryWeightedTimesheetMetricStore(
            new Dictionary<string, WeightedTimesheetMetric>(),
            new List<string>());
    }

    public static IWeightedTimesheetMetricStore NewCachedWeightedTimesheetMetricStore(MemoryWeightedTimesheetMetricStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedTimesheetMetricStore(
            inner,
            WeightedTimesheetMetricCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedTimesheetMetric> FindWeightedTimesheetMetrics(IWeightedTimesheetMetricStore store, WeightedTimesheetMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
