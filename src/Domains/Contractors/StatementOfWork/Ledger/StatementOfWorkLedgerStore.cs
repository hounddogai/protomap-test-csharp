namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work ledgers by code.</summary>
public interface IStatementOfWorkLedgerStore
{
    StatementOfWorkLedger? Find(string code);
    void Save(StatementOfWorkLedger item);
    bool Remove(string code);
    List<StatementOfWorkLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkLedgerStore(Dictionary<string, StatementOfWorkLedger> items, List<string> order) : IStatementOfWorkLedgerStore
{
    public Dictionary<string, StatementOfWorkLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkLedger item)
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

    public List<StatementOfWorkLedger> List()
    {
        var outValue = new List<StatementOfWorkLedger>();
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
public sealed class CachedStatementOfWorkLedgerStore(MemoryStatementOfWorkLedgerStore inner, StatementOfWorkLedgerCache cache, int hits, int misses) : IStatementOfWorkLedgerStore
{
    public MemoryStatementOfWorkLedgerStore Inner { get; set; } = inner;
    public StatementOfWorkLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkLedger? Find(string code)
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

    public void Save(StatementOfWorkLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work ledgers. Empty fields match all.</summary>
public sealed record StatementOfWorkLedgerQuery(string Label, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkLedger item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkLedger> Apply(List<StatementOfWorkLedger> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work ledger store.</summary>
public static class StatementOfWorkLedgerStore
{
    public static IStatementOfWorkLedgerStore NewMemoryStatementOfWorkLedgerStore()
    {
        return new MemoryStatementOfWorkLedgerStore(
            new Dictionary<string, StatementOfWorkLedger>(),
            new List<string>());
    }

    public static IStatementOfWorkLedgerStore NewCachedStatementOfWorkLedgerStore(MemoryStatementOfWorkLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkLedgerStore(inner, StatementOfWorkLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StatementOfWorkLedger> FindStatementOfWorkLedgers(IStatementOfWorkLedgerStore store, StatementOfWorkLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
