namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception metrics by code.</summary>
public interface IExceptionMetricStore
{
    ExceptionMetric? Find(string code);
    void Save(ExceptionMetric item);
    bool Remove(string code);
    List<ExceptionMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionMetricStore(Dictionary<string, ExceptionMetric> items, List<string> order) : IExceptionMetricStore
{
    public Dictionary<string, ExceptionMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionMetric item)
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

    public List<ExceptionMetric> List()
    {
        var outValue = new List<ExceptionMetric>();
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
public sealed class CachedExceptionMetricStore(MemoryExceptionMetricStore inner, ExceptionMetricCache cache, int hits, int misses) : IExceptionMetricStore
{
    public MemoryExceptionMetricStore Inner { get; set; } = inner;
    public ExceptionMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionMetric? Find(string code)
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

    public void Save(ExceptionMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception metrics. Empty fields match all.</summary>
public sealed record ExceptionMetricQuery(string Label, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionMetric> Apply(List<ExceptionMetric> items)
    {
        var matched = Collections.Filter(items, (ExceptionMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception metric store.</summary>
public static class ExceptionMetricStore
{
    public static IExceptionMetricStore NewMemoryExceptionMetricStore()
    {
        return new MemoryExceptionMetricStore(new Dictionary<string, ExceptionMetric>(), new List<string>());
    }

    public static IExceptionMetricStore NewCachedExceptionMetricStore(MemoryExceptionMetricStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionMetricStore(inner, ExceptionMetricCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ExceptionMetric> FindExceptionMetrics(IExceptionMetricStore store, ExceptionMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
