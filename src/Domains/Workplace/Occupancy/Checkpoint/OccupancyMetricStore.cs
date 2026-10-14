namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy metrics by code.</summary>
public interface IOccupancyMetricStore
{
    OccupancyMetric? Find(string code);
    void Save(OccupancyMetric item);
    bool Remove(string code);
    List<OccupancyMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyMetricStore(Dictionary<string, OccupancyMetric> items, List<string> order) : IOccupancyMetricStore
{
    public Dictionary<string, OccupancyMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyMetric item)
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

    public List<OccupancyMetric> List()
    {
        var outValue = new List<OccupancyMetric>();
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
public sealed class CachedOccupancyMetricStore(MemoryOccupancyMetricStore inner, OccupancyMetricCache cache, int hits, int misses) : IOccupancyMetricStore
{
    public MemoryOccupancyMetricStore Inner { get; set; } = inner;
    public OccupancyMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyMetric? Find(string code)
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

    public void Save(OccupancyMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy metrics. Empty fields match all.</summary>
public sealed record OccupancyMetricQuery(string Name, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyMetric item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OccupancyMetric> Apply(List<OccupancyMetric> items)
    {
        var matched = Collections.Filter(items, (OccupancyMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy metric store.</summary>
public static class OccupancyMetricStore
{
    public static IOccupancyMetricStore NewMemoryOccupancyMetricStore()
    {
        return new MemoryOccupancyMetricStore(new Dictionary<string, OccupancyMetric>(), new List<string>());
    }

    public static IOccupancyMetricStore NewCachedOccupancyMetricStore(MemoryOccupancyMetricStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyMetricStore(inner, OccupancyMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OccupancyMetric> FindOccupancyMetrics(IOccupancyMetricStore store, OccupancyMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
