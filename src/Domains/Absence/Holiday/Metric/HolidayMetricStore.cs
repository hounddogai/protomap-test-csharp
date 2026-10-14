namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday metrics by code.</summary>
public interface IHolidayMetricStore
{
    HolidayMetric? Find(string code);
    void Save(HolidayMetric item);
    bool Remove(string code);
    List<HolidayMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayMetricStore(Dictionary<string, HolidayMetric> items, List<string> order) : IHolidayMetricStore
{
    public Dictionary<string, HolidayMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayMetric item)
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

    public List<HolidayMetric> List()
    {
        var outValue = new List<HolidayMetric>();
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
public sealed class CachedHolidayMetricStore(MemoryHolidayMetricStore inner, HolidayMetricCache cache, int hits, int misses) : IHolidayMetricStore
{
    public MemoryHolidayMetricStore Inner { get; set; } = inner;
    public HolidayMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayMetric? Find(string code)
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

    public void Save(HolidayMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday metrics. Empty fields match all.</summary>
public sealed record HolidayMetricQuery(string Department, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayMetric item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayMetric> Apply(List<HolidayMetric> items)
    {
        var matched = Collections.Filter(items, (HolidayMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday metric store.</summary>
public static class HolidayMetricStore
{
    public static IHolidayMetricStore NewMemoryHolidayMetricStore()
    {
        return new MemoryHolidayMetricStore(new Dictionary<string, HolidayMetric>(), new List<string>());
    }

    public static IHolidayMetricStore NewCachedHolidayMetricStore(MemoryHolidayMetricStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayMetricStore(inner, HolidayMetricCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HolidayMetric> FindHolidayMetrics(IHolidayMetricStore store, HolidayMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
