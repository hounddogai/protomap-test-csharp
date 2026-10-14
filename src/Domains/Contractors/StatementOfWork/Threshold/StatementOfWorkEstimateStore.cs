namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work estimates by code.</summary>
public interface IStatementOfWorkEstimateStore
{
    StatementOfWorkEstimate? Find(string code);
    void Save(StatementOfWorkEstimate item);
    bool Remove(string code);
    List<StatementOfWorkEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkEstimateStore(Dictionary<string, StatementOfWorkEstimate> items, List<string> order) : IStatementOfWorkEstimateStore
{
    public Dictionary<string, StatementOfWorkEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkEstimate item)
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

    public List<StatementOfWorkEstimate> List()
    {
        var outValue = new List<StatementOfWorkEstimate>();
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
public sealed class CachedStatementOfWorkEstimateStore(MemoryStatementOfWorkEstimateStore inner, StatementOfWorkEstimateCache cache, int hits, int misses) : IStatementOfWorkEstimateStore
{
    public MemoryStatementOfWorkEstimateStore Inner { get; set; } = inner;
    public StatementOfWorkEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkEstimate? Find(string code)
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

    public void Save(StatementOfWorkEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work estimates. Empty fields match all.</summary>
public sealed record StatementOfWorkEstimateQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkEstimate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkEstimate> Apply(List<StatementOfWorkEstimate> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work estimate store.</summary>
public static class StatementOfWorkEstimateStore
{
    public static IStatementOfWorkEstimateStore NewMemoryStatementOfWorkEstimateStore()
    {
        return new MemoryStatementOfWorkEstimateStore(
            new Dictionary<string, StatementOfWorkEstimate>(),
            new List<string>());
    }

    public static IStatementOfWorkEstimateStore NewCachedStatementOfWorkEstimateStore(MemoryStatementOfWorkEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkEstimateStore(
            inner,
            StatementOfWorkEstimateCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkEstimate> FindStatementOfWorkEstimates(IStatementOfWorkEstimateStore store, StatementOfWorkEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
