namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional pool metrics by code.</summary>
public interface IRegionalPoolMetricStore
{
    RegionalPoolMetric? Find(string code);
    void Save(RegionalPoolMetric item);
    bool Remove(string code);
    List<RegionalPoolMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPoolMetricStore(Dictionary<string, RegionalPoolMetric> items, List<string> order) : IRegionalPoolMetricStore
{
    public Dictionary<string, RegionalPoolMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPoolMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPoolMetric item)
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

    public List<RegionalPoolMetric> List()
    {
        var outValue = new List<RegionalPoolMetric>();
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
public sealed class CachedRegionalPoolMetricStore(MemoryRegionalPoolMetricStore inner, RegionalPoolMetricCache cache, int hits, int misses) : IRegionalPoolMetricStore
{
    public MemoryRegionalPoolMetricStore Inner { get; set; } = inner;
    public RegionalPoolMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPoolMetric? Find(string code)
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

    public void Save(RegionalPoolMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPoolMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional pool metrics. Empty fields match all.</summary>
public sealed record RegionalPoolMetricQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPoolMetric item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<RegionalPoolMetric> Apply(List<RegionalPoolMetric> items)
    {
        var matched = Collections.Filter(items, (RegionalPoolMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional pool metric store.</summary>
public static class RegionalPoolMetricStore
{
    public static IRegionalPoolMetricStore NewMemoryRegionalPoolMetricStore()
    {
        return new MemoryRegionalPoolMetricStore(new Dictionary<string, RegionalPoolMetric>(), new List<string>());
    }

    public static IRegionalPoolMetricStore NewCachedRegionalPoolMetricStore(MemoryRegionalPoolMetricStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPoolMetricStore(inner, RegionalPoolMetricCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RegionalPoolMetric> FindRegionalPoolMetrics(IRegionalPoolMetricStore store, RegionalPoolMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
