namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge metrics by code.</summary>
public interface IAccessBadgeMetricStore
{
    AccessBadgeMetric? Find(string code);
    void Save(AccessBadgeMetric item);
    bool Remove(string code);
    List<AccessBadgeMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeMetricStore(Dictionary<string, AccessBadgeMetric> items, List<string> order) : IAccessBadgeMetricStore
{
    public Dictionary<string, AccessBadgeMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeMetric item)
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

    public List<AccessBadgeMetric> List()
    {
        var outValue = new List<AccessBadgeMetric>();
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
public sealed class CachedAccessBadgeMetricStore(MemoryAccessBadgeMetricStore inner, AccessBadgeMetricCache cache, int hits, int misses) : IAccessBadgeMetricStore
{
    public MemoryAccessBadgeMetricStore Inner { get; set; } = inner;
    public AccessBadgeMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeMetric? Find(string code)
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

    public void Save(AccessBadgeMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge metrics. Empty fields match all.</summary>
public sealed record AccessBadgeMetricQuery(string Label, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeMetric> Apply(List<AccessBadgeMetric> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge metric store.</summary>
public static class AccessBadgeMetricStore
{
    public static IAccessBadgeMetricStore NewMemoryAccessBadgeMetricStore()
    {
        return new MemoryAccessBadgeMetricStore(new Dictionary<string, AccessBadgeMetric>(), new List<string>());
    }

    public static IAccessBadgeMetricStore NewCachedAccessBadgeMetricStore(MemoryAccessBadgeMetricStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeMetricStore(inner, AccessBadgeMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeMetric> FindAccessBadgeMetrics(IAccessBadgeMetricStore store, AccessBadgeMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
