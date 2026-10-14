namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending statement of work variances by code.</summary>
public interface IPendingStatementOfWorkVarianceStore
{
    PendingStatementOfWorkVariance? Find(string code);
    void Save(PendingStatementOfWorkVariance item);
    bool Remove(string code);
    List<PendingStatementOfWorkVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingStatementOfWorkVarianceStore(Dictionary<string, PendingStatementOfWorkVariance> items, List<string> order) : IPendingStatementOfWorkVarianceStore
{
    public Dictionary<string, PendingStatementOfWorkVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingStatementOfWorkVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingStatementOfWorkVariance item)
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

    public List<PendingStatementOfWorkVariance> List()
    {
        var outValue = new List<PendingStatementOfWorkVariance>();
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
public sealed class CachedPendingStatementOfWorkVarianceStore(MemoryPendingStatementOfWorkVarianceStore inner, PendingStatementOfWorkVarianceCache cache, int hits, int misses) : IPendingStatementOfWorkVarianceStore
{
    public MemoryPendingStatementOfWorkVarianceStore Inner { get; set; } = inner;
    public PendingStatementOfWorkVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingStatementOfWorkVariance? Find(string code)
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

    public void Save(PendingStatementOfWorkVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingStatementOfWorkVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending statement of work variances. Empty fields match all.</summary>
public sealed record PendingStatementOfWorkVarianceQuery(string Channel, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(PendingStatementOfWorkVariance item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<PendingStatementOfWorkVariance> Apply(List<PendingStatementOfWorkVariance> items)
    {
        var matched = Collections.Filter(items, (PendingStatementOfWorkVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending statement of work variance store.</summary>
public static class PendingStatementOfWorkVarianceStore
{
    public static IPendingStatementOfWorkVarianceStore NewMemoryPendingStatementOfWorkVarianceStore()
    {
        return new MemoryPendingStatementOfWorkVarianceStore(
            new Dictionary<string, PendingStatementOfWorkVariance>(),
            new List<string>());
    }

    public static IPendingStatementOfWorkVarianceStore NewCachedPendingStatementOfWorkVarianceStore(MemoryPendingStatementOfWorkVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedPendingStatementOfWorkVarianceStore(
            inner,
            PendingStatementOfWorkVarianceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingStatementOfWorkVariance> FindPendingStatementOfWorkVariances(IPendingStatementOfWorkVarianceStore store, PendingStatementOfWorkVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
