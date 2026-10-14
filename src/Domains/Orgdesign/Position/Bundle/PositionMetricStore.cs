namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position metrics by code.</summary>
public interface IPositionMetricStore
{
    PositionMetric? Find(string code);
    void Save(PositionMetric item);
    bool Remove(string code);
    List<PositionMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionMetricStore(Dictionary<string, PositionMetric> items, List<string> order) : IPositionMetricStore
{
    public Dictionary<string, PositionMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionMetric item)
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

    public List<PositionMetric> List()
    {
        var outValue = new List<PositionMetric>();
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
public sealed class CachedPositionMetricStore(MemoryPositionMetricStore inner, PositionMetricCache cache, int hits, int misses) : IPositionMetricStore
{
    public MemoryPositionMetricStore Inner { get; set; } = inner;
    public PositionMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionMetric? Find(string code)
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

    public void Save(PositionMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position metrics. Empty fields match all.</summary>
public sealed record PositionMetricQuery(string Department, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PositionMetric item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionMetric> Apply(List<PositionMetric> items)
    {
        var matched = Collections.Filter(items, (PositionMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position metric store.</summary>
public static class PositionMetricStore
{
    public static IPositionMetricStore NewMemoryPositionMetricStore()
    {
        return new MemoryPositionMetricStore(new Dictionary<string, PositionMetric>(), new List<string>());
    }

    public static IPositionMetricStore NewCachedPositionMetricStore(MemoryPositionMetricStore inner, int ttl, IClock clock)
    {
        return new CachedPositionMetricStore(inner, PositionMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionMetric> FindPositionMetrics(IPositionMetricStore store, PositionMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
