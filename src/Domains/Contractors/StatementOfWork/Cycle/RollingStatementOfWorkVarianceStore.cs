namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling statement of work variances by code.</summary>
public interface IRollingStatementOfWorkVarianceStore
{
    RollingStatementOfWorkVariance? Find(string code);
    void Save(RollingStatementOfWorkVariance item);
    bool Remove(string code);
    List<RollingStatementOfWorkVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingStatementOfWorkVarianceStore(Dictionary<string, RollingStatementOfWorkVariance> items, List<string> order) : IRollingStatementOfWorkVarianceStore
{
    public Dictionary<string, RollingStatementOfWorkVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingStatementOfWorkVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingStatementOfWorkVariance item)
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

    public List<RollingStatementOfWorkVariance> List()
    {
        var outValue = new List<RollingStatementOfWorkVariance>();
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
public sealed class CachedRollingStatementOfWorkVarianceStore(MemoryRollingStatementOfWorkVarianceStore inner, RollingStatementOfWorkVarianceCache cache, int hits, int misses) : IRollingStatementOfWorkVarianceStore
{
    public MemoryRollingStatementOfWorkVarianceStore Inner { get; set; } = inner;
    public RollingStatementOfWorkVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingStatementOfWorkVariance? Find(string code)
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

    public void Save(RollingStatementOfWorkVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingStatementOfWorkVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling statement of work variances. Empty fields match all.</summary>
public sealed record RollingStatementOfWorkVarianceQuery(string Department, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(RollingStatementOfWorkVariance item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingStatementOfWorkVariance> Apply(List<RollingStatementOfWorkVariance> items)
    {
        var matched = Collections.Filter(items, (RollingStatementOfWorkVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling statement of work variance store.</summary>
public static class RollingStatementOfWorkVarianceStore
{
    public static IRollingStatementOfWorkVarianceStore NewMemoryRollingStatementOfWorkVarianceStore()
    {
        return new MemoryRollingStatementOfWorkVarianceStore(
            new Dictionary<string, RollingStatementOfWorkVariance>(),
            new List<string>());
    }

    public static IRollingStatementOfWorkVarianceStore NewCachedRollingStatementOfWorkVarianceStore(MemoryRollingStatementOfWorkVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedRollingStatementOfWorkVarianceStore(
            inner,
            RollingStatementOfWorkVarianceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RollingStatementOfWorkVariance> FindRollingStatementOfWorkVariances(IRollingStatementOfWorkVarianceStore store, RollingStatementOfWorkVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
