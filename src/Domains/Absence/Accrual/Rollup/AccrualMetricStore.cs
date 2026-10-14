namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual metrics by code.</summary>
public interface IAccrualMetricStore
{
    AccrualMetric? Find(string code);
    void Save(AccrualMetric item);
    bool Remove(string code);
    List<AccrualMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualMetricStore(Dictionary<string, AccrualMetric> items, List<string> order) : IAccrualMetricStore
{
    public Dictionary<string, AccrualMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualMetric item)
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

    public List<AccrualMetric> List()
    {
        var outValue = new List<AccrualMetric>();
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
public sealed class CachedAccrualMetricStore(MemoryAccrualMetricStore inner, AccrualMetricCache cache, int hits, int misses) : IAccrualMetricStore
{
    public MemoryAccrualMetricStore Inner { get; set; } = inner;
    public AccrualMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualMetric? Find(string code)
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

    public void Save(AccrualMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual metrics. Empty fields match all.</summary>
public sealed record AccrualMetricQuery(string Department, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualMetric item)
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

    public List<AccrualMetric> Apply(List<AccrualMetric> items)
    {
        var matched = Collections.Filter(items, (AccrualMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual metric store.</summary>
public static class AccrualMetricStore
{
    public static IAccrualMetricStore NewMemoryAccrualMetricStore()
    {
        return new MemoryAccrualMetricStore(new Dictionary<string, AccrualMetric>(), new List<string>());
    }

    public static IAccrualMetricStore NewCachedAccrualMetricStore(MemoryAccrualMetricStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualMetricStore(inner, AccrualMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccrualMetric> FindAccrualMetrics(IAccrualMetricStore store, AccrualMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
