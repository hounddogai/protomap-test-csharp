namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending timesheet metrics by code.</summary>
public interface IPendingTimesheetMetricStore
{
    PendingTimesheetMetric? Find(string code);
    void Save(PendingTimesheetMetric item);
    bool Remove(string code);
    List<PendingTimesheetMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingTimesheetMetricStore(Dictionary<string, PendingTimesheetMetric> items, List<string> order) : IPendingTimesheetMetricStore
{
    public Dictionary<string, PendingTimesheetMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingTimesheetMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingTimesheetMetric item)
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

    public List<PendingTimesheetMetric> List()
    {
        var outValue = new List<PendingTimesheetMetric>();
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
public sealed class CachedPendingTimesheetMetricStore(MemoryPendingTimesheetMetricStore inner, PendingTimesheetMetricCache cache, int hits, int misses) : IPendingTimesheetMetricStore
{
    public MemoryPendingTimesheetMetricStore Inner { get; set; } = inner;
    public PendingTimesheetMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingTimesheetMetric? Find(string code)
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

    public void Save(PendingTimesheetMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingTimesheetMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending timesheet metrics. Empty fields match all.</summary>
public sealed record PendingTimesheetMetricQuery(string Country, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PendingTimesheetMetric item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingTimesheetMetric> Apply(List<PendingTimesheetMetric> items)
    {
        var matched = Collections.Filter(items, (PendingTimesheetMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending timesheet metric store.</summary>
public static class PendingTimesheetMetricStore
{
    public static IPendingTimesheetMetricStore NewMemoryPendingTimesheetMetricStore()
    {
        return new MemoryPendingTimesheetMetricStore(
            new Dictionary<string, PendingTimesheetMetric>(),
            new List<string>());
    }

    public static IPendingTimesheetMetricStore NewCachedPendingTimesheetMetricStore(MemoryPendingTimesheetMetricStore inner, int ttl, IClock clock)
    {
        return new CachedPendingTimesheetMetricStore(
            inner,
            PendingTimesheetMetricCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PendingTimesheetMetric> FindPendingTimesheetMetrics(IPendingTimesheetMetricStore store, PendingTimesheetMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
