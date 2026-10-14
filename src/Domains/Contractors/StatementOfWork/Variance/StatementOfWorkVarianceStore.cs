namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work variances by code.</summary>
public interface IStatementOfWorkVarianceStore
{
    StatementOfWorkVariance? Find(string code);
    void Save(StatementOfWorkVariance item);
    bool Remove(string code);
    List<StatementOfWorkVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkVarianceStore(Dictionary<string, StatementOfWorkVariance> items, List<string> order) : IStatementOfWorkVarianceStore
{
    public Dictionary<string, StatementOfWorkVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkVariance item)
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

    public List<StatementOfWorkVariance> List()
    {
        var outValue = new List<StatementOfWorkVariance>();
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
public sealed class CachedStatementOfWorkVarianceStore(MemoryStatementOfWorkVarianceStore inner, StatementOfWorkVarianceCache cache, int hits, int misses) : IStatementOfWorkVarianceStore
{
    public MemoryStatementOfWorkVarianceStore Inner { get; set; } = inner;
    public StatementOfWorkVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkVariance? Find(string code)
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

    public void Save(StatementOfWorkVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work variances. Empty fields match all.</summary>
public sealed record StatementOfWorkVarianceQuery(string Country, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkVariance item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkVariance> Apply(List<StatementOfWorkVariance> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work variance store.</summary>
public static class StatementOfWorkVarianceStore
{
    public static IStatementOfWorkVarianceStore NewMemoryStatementOfWorkVarianceStore()
    {
        return new MemoryStatementOfWorkVarianceStore(
            new Dictionary<string, StatementOfWorkVariance>(),
            new List<string>());
    }

    public static IStatementOfWorkVarianceStore NewCachedStatementOfWorkVarianceStore(MemoryStatementOfWorkVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkVarianceStore(
            inner,
            StatementOfWorkVarianceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkVariance> FindStatementOfWorkVariances(IStatementOfWorkVarianceStore store, StatementOfWorkVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
