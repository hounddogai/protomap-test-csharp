namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped rate metrics by code.</summary>
public interface ICappedRateMetricStore
{
    CappedRateMetric? Find(string code);
    void Save(CappedRateMetric item);
    bool Remove(string code);
    List<CappedRateMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedRateMetricStore(Dictionary<string, CappedRateMetric> items, List<string> order) : ICappedRateMetricStore
{
    public Dictionary<string, CappedRateMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedRateMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedRateMetric item)
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

    public List<CappedRateMetric> List()
    {
        var outValue = new List<CappedRateMetric>();
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
public sealed class CachedCappedRateMetricStore(MemoryCappedRateMetricStore inner, CappedRateMetricCache cache, int hits, int misses) : ICappedRateMetricStore
{
    public MemoryCappedRateMetricStore Inner { get; set; } = inner;
    public CappedRateMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedRateMetric? Find(string code)
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

    public void Save(CappedRateMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedRateMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped rate metrics. Empty fields match all.</summary>
public sealed record CappedRateMetricQuery(string Unit, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(CappedRateMetric item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedRateMetric> Apply(List<CappedRateMetric> items)
    {
        var matched = Collections.Filter(items, (CappedRateMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped rate metric store.</summary>
public static class CappedRateMetricStore
{
    public static ICappedRateMetricStore NewMemoryCappedRateMetricStore()
    {
        return new MemoryCappedRateMetricStore(new Dictionary<string, CappedRateMetric>(), new List<string>());
    }

    public static ICappedRateMetricStore NewCachedCappedRateMetricStore(MemoryCappedRateMetricStore inner, int ttl, IClock clock)
    {
        return new CachedCappedRateMetricStore(inner, CappedRateMetricCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CappedRateMetric> FindCappedRateMetrics(ICappedRateMetricStore store, CappedRateMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
