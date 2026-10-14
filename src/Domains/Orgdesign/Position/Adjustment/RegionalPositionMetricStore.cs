namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional position metrics by code.</summary>
public interface IRegionalPositionMetricStore
{
    RegionalPositionMetric? Find(string code);
    void Save(RegionalPositionMetric item);
    bool Remove(string code);
    List<RegionalPositionMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPositionMetricStore(Dictionary<string, RegionalPositionMetric> items, List<string> order) : IRegionalPositionMetricStore
{
    public Dictionary<string, RegionalPositionMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPositionMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPositionMetric item)
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

    public List<RegionalPositionMetric> List()
    {
        var outValue = new List<RegionalPositionMetric>();
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
public sealed class CachedRegionalPositionMetricStore(MemoryRegionalPositionMetricStore inner, RegionalPositionMetricCache cache, int hits, int misses) : IRegionalPositionMetricStore
{
    public MemoryRegionalPositionMetricStore Inner { get; set; } = inner;
    public RegionalPositionMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPositionMetric? Find(string code)
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

    public void Save(RegionalPositionMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPositionMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional position metrics. Empty fields match all.</summary>
public sealed record RegionalPositionMetricQuery(string Title, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPositionMetric item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalPositionMetric> Apply(List<RegionalPositionMetric> items)
    {
        var matched = Collections.Filter(items, (RegionalPositionMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional position metric store.</summary>
public static class RegionalPositionMetricStore
{
    public static IRegionalPositionMetricStore NewMemoryRegionalPositionMetricStore()
    {
        return new MemoryRegionalPositionMetricStore(
            new Dictionary<string, RegionalPositionMetric>(),
            new List<string>());
    }

    public static IRegionalPositionMetricStore NewCachedRegionalPositionMetricStore(MemoryRegionalPositionMetricStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPositionMetricStore(
            inner,
            RegionalPositionMetricCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalPositionMetric> FindRegionalPositionMetrics(IRegionalPositionMetricStore store, RegionalPositionMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
