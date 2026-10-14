namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted audit metrics by code.</summary>
public interface IWeightedAuditMetricStore
{
    WeightedAuditMetric? Find(string code);
    void Save(WeightedAuditMetric item);
    bool Remove(string code);
    List<WeightedAuditMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAuditMetricStore(Dictionary<string, WeightedAuditMetric> items, List<string> order) : IWeightedAuditMetricStore
{
    public Dictionary<string, WeightedAuditMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAuditMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAuditMetric item)
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

    public List<WeightedAuditMetric> List()
    {
        var outValue = new List<WeightedAuditMetric>();
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
public sealed class CachedWeightedAuditMetricStore(MemoryWeightedAuditMetricStore inner, WeightedAuditMetricCache cache, int hits, int misses) : IWeightedAuditMetricStore
{
    public MemoryWeightedAuditMetricStore Inner { get; set; } = inner;
    public WeightedAuditMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAuditMetric? Find(string code)
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

    public void Save(WeightedAuditMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAuditMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted audit metrics. Empty fields match all.</summary>
public sealed record WeightedAuditMetricQuery(string Segment, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAuditMetric item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAuditMetric> Apply(List<WeightedAuditMetric> items)
    {
        var matched = Collections.Filter(items, (WeightedAuditMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted audit metric store.</summary>
public static class WeightedAuditMetricStore
{
    public static IWeightedAuditMetricStore NewMemoryWeightedAuditMetricStore()
    {
        return new MemoryWeightedAuditMetricStore(new Dictionary<string, WeightedAuditMetric>(), new List<string>());
    }

    public static IWeightedAuditMetricStore NewCachedWeightedAuditMetricStore(MemoryWeightedAuditMetricStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAuditMetricStore(inner, WeightedAuditMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<WeightedAuditMetric> FindWeightedAuditMetrics(IWeightedAuditMetricStore store, WeightedAuditMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
