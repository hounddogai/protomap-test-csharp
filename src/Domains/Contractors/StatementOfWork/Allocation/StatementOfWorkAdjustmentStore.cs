namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work adjustments by code.</summary>
public interface IStatementOfWorkAdjustmentStore
{
    StatementOfWorkAdjustment? Find(string code);
    void Save(StatementOfWorkAdjustment item);
    bool Remove(string code);
    List<StatementOfWorkAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkAdjustmentStore(Dictionary<string, StatementOfWorkAdjustment> items, List<string> order) : IStatementOfWorkAdjustmentStore
{
    public Dictionary<string, StatementOfWorkAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkAdjustment item)
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

    public List<StatementOfWorkAdjustment> List()
    {
        var outValue = new List<StatementOfWorkAdjustment>();
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
public sealed class CachedStatementOfWorkAdjustmentStore(MemoryStatementOfWorkAdjustmentStore inner, StatementOfWorkAdjustmentCache cache, int hits, int misses) : IStatementOfWorkAdjustmentStore
{
    public MemoryStatementOfWorkAdjustmentStore Inner { get; set; } = inner;
    public StatementOfWorkAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkAdjustment? Find(string code)
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

    public void Save(StatementOfWorkAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work adjustments. Empty fields match all.</summary>
public sealed record StatementOfWorkAdjustmentQuery(string Department, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkAdjustment item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<StatementOfWorkAdjustment> Apply(List<StatementOfWorkAdjustment> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work adjustment store.</summary>
public static class StatementOfWorkAdjustmentStore
{
    public static IStatementOfWorkAdjustmentStore NewMemoryStatementOfWorkAdjustmentStore()
    {
        return new MemoryStatementOfWorkAdjustmentStore(
            new Dictionary<string, StatementOfWorkAdjustment>(),
            new List<string>());
    }

    public static IStatementOfWorkAdjustmentStore NewCachedStatementOfWorkAdjustmentStore(MemoryStatementOfWorkAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkAdjustmentStore(
            inner,
            StatementOfWorkAdjustmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkAdjustment> FindStatementOfWorkAdjustments(IStatementOfWorkAdjustmentStore store, StatementOfWorkAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
