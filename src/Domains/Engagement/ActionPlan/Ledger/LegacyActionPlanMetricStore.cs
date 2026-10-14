namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy action plan metrics by code.</summary>
public interface ILegacyActionPlanMetricStore
{
    LegacyActionPlanMetric? Find(string code);
    void Save(LegacyActionPlanMetric item);
    bool Remove(string code);
    List<LegacyActionPlanMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyActionPlanMetricStore(Dictionary<string, LegacyActionPlanMetric> items, List<string> order) : ILegacyActionPlanMetricStore
{
    public Dictionary<string, LegacyActionPlanMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyActionPlanMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyActionPlanMetric item)
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

    public List<LegacyActionPlanMetric> List()
    {
        var outValue = new List<LegacyActionPlanMetric>();
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
public sealed class CachedLegacyActionPlanMetricStore(MemoryLegacyActionPlanMetricStore inner, LegacyActionPlanMetricCache cache, int hits, int misses) : ILegacyActionPlanMetricStore
{
    public MemoryLegacyActionPlanMetricStore Inner { get; set; } = inner;
    public LegacyActionPlanMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyActionPlanMetric? Find(string code)
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

    public void Save(LegacyActionPlanMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyActionPlanMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy action plan metrics. Empty fields match all.</summary>
public sealed record LegacyActionPlanMetricQuery(string Label, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyActionPlanMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyActionPlanMetric> Apply(List<LegacyActionPlanMetric> items)
    {
        var matched = Collections.Filter(items, (LegacyActionPlanMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy action plan metric store.</summary>
public static class LegacyActionPlanMetricStore
{
    public static ILegacyActionPlanMetricStore NewMemoryLegacyActionPlanMetricStore()
    {
        return new MemoryLegacyActionPlanMetricStore(
            new Dictionary<string, LegacyActionPlanMetric>(),
            new List<string>());
    }

    public static ILegacyActionPlanMetricStore NewCachedLegacyActionPlanMetricStore(MemoryLegacyActionPlanMetricStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyActionPlanMetricStore(
            inner,
            LegacyActionPlanMetricCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LegacyActionPlanMetric> FindLegacyActionPlanMetrics(ILegacyActionPlanMetricStore store, LegacyActionPlanMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
