namespace Acme.Workforce.Contractors.Timesheet.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy timesheet metrics by code.</summary>
public interface ILegacyTimesheetMetricStore
{
    LegacyTimesheetMetric? Find(string code);
    void Save(LegacyTimesheetMetric item);
    bool Remove(string code);
    List<LegacyTimesheetMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyTimesheetMetricStore(Dictionary<string, LegacyTimesheetMetric> items, List<string> order) : ILegacyTimesheetMetricStore
{
    public Dictionary<string, LegacyTimesheetMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyTimesheetMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyTimesheetMetric item)
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

    public List<LegacyTimesheetMetric> List()
    {
        var outValue = new List<LegacyTimesheetMetric>();
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
public sealed class CachedLegacyTimesheetMetricStore(MemoryLegacyTimesheetMetricStore inner, LegacyTimesheetMetricCache cache, int hits, int misses) : ILegacyTimesheetMetricStore
{
    public MemoryLegacyTimesheetMetricStore Inner { get; set; } = inner;
    public LegacyTimesheetMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyTimesheetMetric? Find(string code)
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

    public void Save(LegacyTimesheetMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyTimesheetMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy timesheet metrics. Empty fields match all.</summary>
public sealed record LegacyTimesheetMetricQuery(string Segment, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyTimesheetMetric item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyTimesheetMetric> Apply(List<LegacyTimesheetMetric> items)
    {
        var matched = Collections.Filter(items, (LegacyTimesheetMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy timesheet metric store.</summary>
public static class LegacyTimesheetMetricStore
{
    public static ILegacyTimesheetMetricStore NewMemoryLegacyTimesheetMetricStore()
    {
        return new MemoryLegacyTimesheetMetricStore(
            new Dictionary<string, LegacyTimesheetMetric>(),
            new List<string>());
    }

    public static ILegacyTimesheetMetricStore NewCachedLegacyTimesheetMetricStore(MemoryLegacyTimesheetMetricStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyTimesheetMetricStore(inner, LegacyTimesheetMetricCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LegacyTimesheetMetric> FindLegacyTimesheetMetrics(ILegacyTimesheetMetricStore store, LegacyTimesheetMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
