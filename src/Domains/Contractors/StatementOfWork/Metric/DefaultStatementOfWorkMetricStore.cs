namespace Acme.Workforce.Contractors.StatementOfWork.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default statement of work metrics by code.</summary>
public interface IDefaultStatementOfWorkMetricStore
{
    DefaultStatementOfWorkMetric? Find(string code);
    void Save(DefaultStatementOfWorkMetric item);
    bool Remove(string code);
    List<DefaultStatementOfWorkMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultStatementOfWorkMetricStore(Dictionary<string, DefaultStatementOfWorkMetric> items, List<string> order) : IDefaultStatementOfWorkMetricStore
{
    public Dictionary<string, DefaultStatementOfWorkMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultStatementOfWorkMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultStatementOfWorkMetric item)
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

    public List<DefaultStatementOfWorkMetric> List()
    {
        var outValue = new List<DefaultStatementOfWorkMetric>();
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
public sealed class CachedDefaultStatementOfWorkMetricStore(MemoryDefaultStatementOfWorkMetricStore inner, DefaultStatementOfWorkMetricCache cache, int hits, int misses) : IDefaultStatementOfWorkMetricStore
{
    public MemoryDefaultStatementOfWorkMetricStore Inner { get; set; } = inner;
    public DefaultStatementOfWorkMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultStatementOfWorkMetric? Find(string code)
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

    public void Save(DefaultStatementOfWorkMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultStatementOfWorkMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default statement of work metrics. Empty fields match all.</summary>
public sealed record DefaultStatementOfWorkMetricQuery(string Country, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultStatementOfWorkMetric item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<DefaultStatementOfWorkMetric> Apply(List<DefaultStatementOfWorkMetric> items)
    {
        var matched = Collections.Filter(items, (DefaultStatementOfWorkMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default statement of work metric store.</summary>
public static class DefaultStatementOfWorkMetricStore
{
    public static IDefaultStatementOfWorkMetricStore NewMemoryDefaultStatementOfWorkMetricStore()
    {
        return new MemoryDefaultStatementOfWorkMetricStore(
            new Dictionary<string, DefaultStatementOfWorkMetric>(),
            new List<string>());
    }

    public static IDefaultStatementOfWorkMetricStore NewCachedDefaultStatementOfWorkMetricStore(MemoryDefaultStatementOfWorkMetricStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultStatementOfWorkMetricStore(
            inner,
            DefaultStatementOfWorkMetricCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultStatementOfWorkMetric> FindDefaultStatementOfWorkMetrics(IDefaultStatementOfWorkMetricStore store, DefaultStatementOfWorkMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
