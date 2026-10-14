namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work metrics by code.</summary>
public interface IStatementOfWorkMetricStore
{
    StatementOfWorkMetric? Find(string code);
    void Save(StatementOfWorkMetric item);
    bool Remove(string code);
    List<StatementOfWorkMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkMetricStore(Dictionary<string, StatementOfWorkMetric> items, List<string> order) : IStatementOfWorkMetricStore
{
    public Dictionary<string, StatementOfWorkMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkMetric item)
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

    public List<StatementOfWorkMetric> List()
    {
        var outValue = new List<StatementOfWorkMetric>();
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
public sealed class CachedStatementOfWorkMetricStore(MemoryStatementOfWorkMetricStore inner, StatementOfWorkMetricCache cache, int hits, int misses) : IStatementOfWorkMetricStore
{
    public MemoryStatementOfWorkMetricStore Inner { get; set; } = inner;
    public StatementOfWorkMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkMetric? Find(string code)
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

    public void Save(StatementOfWorkMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work metrics. Empty fields match all.</summary>
public sealed record StatementOfWorkMetricQuery(string Team, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkMetric item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkMetric> Apply(List<StatementOfWorkMetric> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work metric store.</summary>
public static class StatementOfWorkMetricStore
{
    public static IStatementOfWorkMetricStore NewMemoryStatementOfWorkMetricStore()
    {
        return new MemoryStatementOfWorkMetricStore(
            new Dictionary<string, StatementOfWorkMetric>(),
            new List<string>());
    }

    public static IStatementOfWorkMetricStore NewCachedStatementOfWorkMetricStore(MemoryStatementOfWorkMetricStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkMetricStore(inner, StatementOfWorkMetricCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkMetric> FindStatementOfWorkMetrics(IStatementOfWorkMetricStore store, StatementOfWorkMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
