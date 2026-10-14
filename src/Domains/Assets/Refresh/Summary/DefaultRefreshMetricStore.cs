namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default refresh metrics by code.</summary>
public interface IDefaultRefreshMetricStore
{
    DefaultRefreshMetric? Find(string code);
    void Save(DefaultRefreshMetric item);
    bool Remove(string code);
    List<DefaultRefreshMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultRefreshMetricStore(Dictionary<string, DefaultRefreshMetric> items, List<string> order) : IDefaultRefreshMetricStore
{
    public Dictionary<string, DefaultRefreshMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultRefreshMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultRefreshMetric item)
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

    public List<DefaultRefreshMetric> List()
    {
        var outValue = new List<DefaultRefreshMetric>();
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
public sealed class CachedDefaultRefreshMetricStore(MemoryDefaultRefreshMetricStore inner, DefaultRefreshMetricCache cache, int hits, int misses) : IDefaultRefreshMetricStore
{
    public MemoryDefaultRefreshMetricStore Inner { get; set; } = inner;
    public DefaultRefreshMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultRefreshMetric? Find(string code)
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

    public void Save(DefaultRefreshMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultRefreshMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default refresh metrics. Empty fields match all.</summary>
public sealed record DefaultRefreshMetricQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultRefreshMetric item)
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

    public List<DefaultRefreshMetric> Apply(List<DefaultRefreshMetric> items)
    {
        var matched = Collections.Filter(items, (DefaultRefreshMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default refresh metric store.</summary>
public static class DefaultRefreshMetricStore
{
    public static IDefaultRefreshMetricStore NewMemoryDefaultRefreshMetricStore()
    {
        return new MemoryDefaultRefreshMetricStore(
            new Dictionary<string, DefaultRefreshMetric>(),
            new List<string>());
    }

    public static IDefaultRefreshMetricStore NewCachedDefaultRefreshMetricStore(MemoryDefaultRefreshMetricStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultRefreshMetricStore(inner, DefaultRefreshMetricCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DefaultRefreshMetric> FindDefaultRefreshMetrics(IDefaultRefreshMetricStore store, DefaultRefreshMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
