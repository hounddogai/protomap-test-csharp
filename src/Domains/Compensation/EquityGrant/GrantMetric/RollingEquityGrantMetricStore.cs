namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling equity grant metrics by code.</summary>
public interface IRollingEquityGrantMetricStore
{
    RollingEquityGrantMetric? Find(string code);
    void Save(RollingEquityGrantMetric item);
    bool Remove(string code);
    List<RollingEquityGrantMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingEquityGrantMetricStore(Dictionary<string, RollingEquityGrantMetric> items, List<string> order) : IRollingEquityGrantMetricStore
{
    public Dictionary<string, RollingEquityGrantMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingEquityGrantMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingEquityGrantMetric item)
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

    public List<RollingEquityGrantMetric> List()
    {
        var outValue = new List<RollingEquityGrantMetric>();
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
public sealed class CachedRollingEquityGrantMetricStore(MemoryRollingEquityGrantMetricStore inner, RollingEquityGrantMetricCache cache, int hits, int misses) : IRollingEquityGrantMetricStore
{
    public MemoryRollingEquityGrantMetricStore Inner { get; set; } = inner;
    public RollingEquityGrantMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingEquityGrantMetric? Find(string code)
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

    public void Save(RollingEquityGrantMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingEquityGrantMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling equity grant metrics. Empty fields match all.</summary>
public sealed record RollingEquityGrantMetricQuery(string Source, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(RollingEquityGrantMetric item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingEquityGrantMetric> Apply(List<RollingEquityGrantMetric> items)
    {
        var matched = Collections.Filter(items, (RollingEquityGrantMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling equity grant metric store.</summary>
public static class RollingEquityGrantMetricStore
{
    public static IRollingEquityGrantMetricStore NewMemoryRollingEquityGrantMetricStore()
    {
        return new MemoryRollingEquityGrantMetricStore(
            new Dictionary<string, RollingEquityGrantMetric>(),
            new List<string>());
    }

    public static IRollingEquityGrantMetricStore NewCachedRollingEquityGrantMetricStore(MemoryRollingEquityGrantMetricStore inner, int ttl, IClock clock)
    {
        return new CachedRollingEquityGrantMetricStore(
            inner,
            RollingEquityGrantMetricCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RollingEquityGrantMetric> FindRollingEquityGrantMetrics(IRollingEquityGrantMetricStore store, RollingEquityGrantMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
