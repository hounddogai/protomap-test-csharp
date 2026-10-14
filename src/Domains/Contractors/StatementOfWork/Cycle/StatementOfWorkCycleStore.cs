namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work cycles by code.</summary>
public interface IStatementOfWorkCycleStore
{
    StatementOfWorkCycle? Find(string code);
    void Save(StatementOfWorkCycle item);
    bool Remove(string code);
    List<StatementOfWorkCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkCycleStore(Dictionary<string, StatementOfWorkCycle> items, List<string> order) : IStatementOfWorkCycleStore
{
    public Dictionary<string, StatementOfWorkCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkCycle item)
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

    public List<StatementOfWorkCycle> List()
    {
        var outValue = new List<StatementOfWorkCycle>();
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
public sealed class CachedStatementOfWorkCycleStore(MemoryStatementOfWorkCycleStore inner, StatementOfWorkCycleCache cache, int hits, int misses) : IStatementOfWorkCycleStore
{
    public MemoryStatementOfWorkCycleStore Inner { get; set; } = inner;
    public StatementOfWorkCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkCycle? Find(string code)
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

    public void Save(StatementOfWorkCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work cycles. Empty fields match all.</summary>
public sealed record StatementOfWorkCycleQuery(string Label, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkCycle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<StatementOfWorkCycle> Apply(List<StatementOfWorkCycle> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work cycle store.</summary>
public static class StatementOfWorkCycleStore
{
    public static IStatementOfWorkCycleStore NewMemoryStatementOfWorkCycleStore()
    {
        return new MemoryStatementOfWorkCycleStore(
            new Dictionary<string, StatementOfWorkCycle>(),
            new List<string>());
    }

    public static IStatementOfWorkCycleStore NewCachedStatementOfWorkCycleStore(MemoryStatementOfWorkCycleStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkCycleStore(inner, StatementOfWorkCycleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkCycle> FindStatementOfWorkCycles(IStatementOfWorkCycleStore store, StatementOfWorkCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
