namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict statement of work metrics by code.</summary>
public interface IStrictStatementOfWorkMetricStore
{
    StrictStatementOfWorkMetric? Find(string code);
    void Save(StrictStatementOfWorkMetric item);
    bool Remove(string code);
    List<StrictStatementOfWorkMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictStatementOfWorkMetricStore(Dictionary<string, StrictStatementOfWorkMetric> items, List<string> order) : IStrictStatementOfWorkMetricStore
{
    public Dictionary<string, StrictStatementOfWorkMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictStatementOfWorkMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictStatementOfWorkMetric item)
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

    public List<StrictStatementOfWorkMetric> List()
    {
        var outValue = new List<StrictStatementOfWorkMetric>();
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
public sealed class CachedStrictStatementOfWorkMetricStore(MemoryStrictStatementOfWorkMetricStore inner, StrictStatementOfWorkMetricCache cache, int hits, int misses) : IStrictStatementOfWorkMetricStore
{
    public MemoryStrictStatementOfWorkMetricStore Inner { get; set; } = inner;
    public StrictStatementOfWorkMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictStatementOfWorkMetric? Find(string code)
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

    public void Save(StrictStatementOfWorkMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictStatementOfWorkMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict statement of work metrics. Empty fields match all.</summary>
public sealed record StrictStatementOfWorkMetricQuery(string Region, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(StrictStatementOfWorkMetric item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictStatementOfWorkMetric> Apply(List<StrictStatementOfWorkMetric> items)
    {
        var matched = Collections.Filter(items, (StrictStatementOfWorkMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict statement of work metric store.</summary>
public static class StrictStatementOfWorkMetricStore
{
    public static IStrictStatementOfWorkMetricStore NewMemoryStrictStatementOfWorkMetricStore()
    {
        return new MemoryStrictStatementOfWorkMetricStore(
            new Dictionary<string, StrictStatementOfWorkMetric>(),
            new List<string>());
    }

    public static IStrictStatementOfWorkMetricStore NewCachedStrictStatementOfWorkMetricStore(MemoryStrictStatementOfWorkMetricStore inner, int ttl, IClock clock)
    {
        return new CachedStrictStatementOfWorkMetricStore(
            inner,
            StrictStatementOfWorkMetricCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<StrictStatementOfWorkMetric> FindStrictStatementOfWorkMetrics(IStrictStatementOfWorkMetricStore store, StrictStatementOfWorkMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
