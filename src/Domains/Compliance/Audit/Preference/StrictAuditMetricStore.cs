namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict audit metrics by code.</summary>
public interface IStrictAuditMetricStore
{
    StrictAuditMetric? Find(string code);
    void Save(StrictAuditMetric item);
    bool Remove(string code);
    List<StrictAuditMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictAuditMetricStore(Dictionary<string, StrictAuditMetric> items, List<string> order) : IStrictAuditMetricStore
{
    public Dictionary<string, StrictAuditMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictAuditMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictAuditMetric item)
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

    public List<StrictAuditMetric> List()
    {
        var outValue = new List<StrictAuditMetric>();
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
public sealed class CachedStrictAuditMetricStore(MemoryStrictAuditMetricStore inner, StrictAuditMetricCache cache, int hits, int misses) : IStrictAuditMetricStore
{
    public MemoryStrictAuditMetricStore Inner { get; set; } = inner;
    public StrictAuditMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictAuditMetric? Find(string code)
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

    public void Save(StrictAuditMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictAuditMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict audit metrics. Empty fields match all.</summary>
public sealed record StrictAuditMetricQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(StrictAuditMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictAuditMetric> Apply(List<StrictAuditMetric> items)
    {
        var matched = Collections.Filter(items, (StrictAuditMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict audit metric store.</summary>
public static class StrictAuditMetricStore
{
    public static IStrictAuditMetricStore NewMemoryStrictAuditMetricStore()
    {
        return new MemoryStrictAuditMetricStore(new Dictionary<string, StrictAuditMetric>(), new List<string>());
    }

    public static IStrictAuditMetricStore NewCachedStrictAuditMetricStore(MemoryStrictAuditMetricStore inner, int ttl, IClock clock)
    {
        return new CachedStrictAuditMetricStore(inner, StrictAuditMetricCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StrictAuditMetric> FindStrictAuditMetrics(IStrictAuditMetricStore store, StrictAuditMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
