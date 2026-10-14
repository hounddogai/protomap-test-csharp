namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped equity grant metrics by code.</summary>
public interface ICappedEquityGrantMetricStore
{
    CappedEquityGrantMetric? Find(string code);
    void Save(CappedEquityGrantMetric item);
    bool Remove(string code);
    List<CappedEquityGrantMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedEquityGrantMetricStore(Dictionary<string, CappedEquityGrantMetric> items, List<string> order) : ICappedEquityGrantMetricStore
{
    public Dictionary<string, CappedEquityGrantMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedEquityGrantMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedEquityGrantMetric item)
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

    public List<CappedEquityGrantMetric> List()
    {
        var outValue = new List<CappedEquityGrantMetric>();
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
public sealed class CachedCappedEquityGrantMetricStore(MemoryCappedEquityGrantMetricStore inner, CappedEquityGrantMetricCache cache, int hits, int misses) : ICappedEquityGrantMetricStore
{
    public MemoryCappedEquityGrantMetricStore Inner { get; set; } = inner;
    public CappedEquityGrantMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedEquityGrantMetric? Find(string code)
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

    public void Save(CappedEquityGrantMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedEquityGrantMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped equity grant metrics. Empty fields match all.</summary>
public sealed record CappedEquityGrantMetricQuery(string Category, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(CappedEquityGrantMetric item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedEquityGrantMetric> Apply(List<CappedEquityGrantMetric> items)
    {
        var matched = Collections.Filter(items, (CappedEquityGrantMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped equity grant metric store.</summary>
public static class CappedEquityGrantMetricStore
{
    public static ICappedEquityGrantMetricStore NewMemoryCappedEquityGrantMetricStore()
    {
        return new MemoryCappedEquityGrantMetricStore(
            new Dictionary<string, CappedEquityGrantMetric>(),
            new List<string>());
    }

    public static ICappedEquityGrantMetricStore NewCachedCappedEquityGrantMetricStore(MemoryCappedEquityGrantMetricStore inner, int ttl, IClock clock)
    {
        return new CachedCappedEquityGrantMetricStore(
            inner,
            CappedEquityGrantMetricCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedEquityGrantMetric> FindCappedEquityGrantMetrics(ICappedEquityGrantMetricStore store, CappedEquityGrantMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
