namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span metrics by code.</summary>
public interface ISpanMetricStore
{
    SpanMetric? Find(string code);
    void Save(SpanMetric item);
    bool Remove(string code);
    List<SpanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanMetricStore(Dictionary<string, SpanMetric> items, List<string> order) : ISpanMetricStore
{
    public Dictionary<string, SpanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanMetric item)
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

    public List<SpanMetric> List()
    {
        var outValue = new List<SpanMetric>();
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
public sealed class CachedSpanMetricStore(MemorySpanMetricStore inner, SpanMetricCache cache, int hits, int misses) : ISpanMetricStore
{
    public MemorySpanMetricStore Inner { get; set; } = inner;
    public SpanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanMetric? Find(string code)
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

    public void Save(SpanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span metrics. Empty fields match all.</summary>
public sealed record SpanMetricQuery(string Department, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(SpanMetric item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanMetric> Apply(List<SpanMetric> items)
    {
        var matched = Collections.Filter(items, (SpanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span metric store.</summary>
public static class SpanMetricStore
{
    public static ISpanMetricStore NewMemorySpanMetricStore()
    {
        return new MemorySpanMetricStore(new Dictionary<string, SpanMetric>(), new List<string>());
    }

    public static ISpanMetricStore NewCachedSpanMetricStore(MemorySpanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedSpanMetricStore(inner, SpanMetricCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SpanMetric> FindSpanMetrics(ISpanMetricStore store, SpanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
