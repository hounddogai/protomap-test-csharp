namespace Acme.Workforce.Compliance.Audit.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit metrics by code.</summary>
public interface IAuditMetricStore
{
    AuditMetric? Find(string code);
    void Save(AuditMetric item);
    bool Remove(string code);
    List<AuditMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditMetricStore(Dictionary<string, AuditMetric> items, List<string> order) : IAuditMetricStore
{
    public Dictionary<string, AuditMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditMetric item)
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

    public List<AuditMetric> List()
    {
        var outValue = new List<AuditMetric>();
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
public sealed class CachedAuditMetricStore(MemoryAuditMetricStore inner, AuditMetricCache cache, int hits, int misses) : IAuditMetricStore
{
    public MemoryAuditMetricStore Inner { get; set; } = inner;
    public AuditMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditMetric? Find(string code)
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

    public void Save(AuditMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit metrics. Empty fields match all.</summary>
public sealed record AuditMetricQuery(string Source, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AuditMetric item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditMetric> Apply(List<AuditMetric> items)
    {
        var matched = Collections.Filter(items, (AuditMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit metric store.</summary>
public static class AuditMetricStore
{
    public static IAuditMetricStore NewMemoryAuditMetricStore()
    {
        return new MemoryAuditMetricStore(new Dictionary<string, AuditMetric>(), new List<string>());
    }

    public static IAuditMetricStore NewCachedAuditMetricStore(MemoryAuditMetricStore inner, int ttl, IClock clock)
    {
        return new CachedAuditMetricStore(inner, AuditMetricCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AuditMetric> FindAuditMetrics(IAuditMetricStore store, AuditMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
