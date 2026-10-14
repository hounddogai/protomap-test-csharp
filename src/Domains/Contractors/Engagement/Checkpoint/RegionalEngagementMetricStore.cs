namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional engagement metrics by code.</summary>
public interface IRegionalEngagementMetricStore
{
    RegionalEngagementMetric? Find(string code);
    void Save(RegionalEngagementMetric item);
    bool Remove(string code);
    List<RegionalEngagementMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalEngagementMetricStore(Dictionary<string, RegionalEngagementMetric> items, List<string> order) : IRegionalEngagementMetricStore
{
    public Dictionary<string, RegionalEngagementMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalEngagementMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalEngagementMetric item)
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

    public List<RegionalEngagementMetric> List()
    {
        var outValue = new List<RegionalEngagementMetric>();
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
public sealed class CachedRegionalEngagementMetricStore(MemoryRegionalEngagementMetricStore inner, RegionalEngagementMetricCache cache, int hits, int misses) : IRegionalEngagementMetricStore
{
    public MemoryRegionalEngagementMetricStore Inner { get; set; } = inner;
    public RegionalEngagementMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalEngagementMetric? Find(string code)
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

    public void Save(RegionalEngagementMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalEngagementMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional engagement metrics. Empty fields match all.</summary>
public sealed record RegionalEngagementMetricQuery(string Country, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalEngagementMetric item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalEngagementMetric> Apply(List<RegionalEngagementMetric> items)
    {
        var matched = Collections.Filter(items, (RegionalEngagementMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional engagement metric store.</summary>
public static class RegionalEngagementMetricStore
{
    public static IRegionalEngagementMetricStore NewMemoryRegionalEngagementMetricStore()
    {
        return new MemoryRegionalEngagementMetricStore(
            new Dictionary<string, RegionalEngagementMetric>(),
            new List<string>());
    }

    public static IRegionalEngagementMetricStore NewCachedRegionalEngagementMetricStore(MemoryRegionalEngagementMetricStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalEngagementMetricStore(
            inner,
            RegionalEngagementMetricCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalEngagementMetric> FindRegionalEngagementMetrics(IRegionalEngagementMetricStore store, RegionalEngagementMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
