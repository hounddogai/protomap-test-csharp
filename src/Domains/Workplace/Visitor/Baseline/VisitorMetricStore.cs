namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor metrics by code.</summary>
public interface IVisitorMetricStore
{
    VisitorMetric? Find(string code);
    void Save(VisitorMetric item);
    bool Remove(string code);
    List<VisitorMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorMetricStore(Dictionary<string, VisitorMetric> items, List<string> order) : IVisitorMetricStore
{
    public Dictionary<string, VisitorMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorMetric item)
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

    public List<VisitorMetric> List()
    {
        var outValue = new List<VisitorMetric>();
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
public sealed class CachedVisitorMetricStore(MemoryVisitorMetricStore inner, VisitorMetricCache cache, int hits, int misses) : IVisitorMetricStore
{
    public MemoryVisitorMetricStore Inner { get; set; } = inner;
    public VisitorMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorMetric? Find(string code)
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

    public void Save(VisitorMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor metrics. Empty fields match all.</summary>
public sealed record VisitorMetricQuery(string Locale, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorMetric item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorMetric> Apply(List<VisitorMetric> items)
    {
        var matched = Collections.Filter(items, (VisitorMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor metric store.</summary>
public static class VisitorMetricStore
{
    public static IVisitorMetricStore NewMemoryVisitorMetricStore()
    {
        return new MemoryVisitorMetricStore(new Dictionary<string, VisitorMetric>(), new List<string>());
    }

    public static IVisitorMetricStore NewCachedVisitorMetricStore(MemoryVisitorMetricStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorMetricStore(inner, VisitorMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<VisitorMetric> FindVisitorMetrics(IVisitorMetricStore store, VisitorMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
