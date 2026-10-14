namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps statement of work agreements by code.</summary>
public interface IStatementOfWorkAgreementStore
{
    StatementOfWorkAgreement? Find(string code);
    void Save(StatementOfWorkAgreement item);
    bool Remove(string code);
    List<StatementOfWorkAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStatementOfWorkAgreementStore(Dictionary<string, StatementOfWorkAgreement> items, List<string> order) : IStatementOfWorkAgreementStore
{
    public Dictionary<string, StatementOfWorkAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StatementOfWorkAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StatementOfWorkAgreement item)
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

    public List<StatementOfWorkAgreement> List()
    {
        var outValue = new List<StatementOfWorkAgreement>();
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
public sealed class CachedStatementOfWorkAgreementStore(MemoryStatementOfWorkAgreementStore inner, StatementOfWorkAgreementCache cache, int hits, int misses) : IStatementOfWorkAgreementStore
{
    public MemoryStatementOfWorkAgreementStore Inner { get; set; } = inner;
    public StatementOfWorkAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StatementOfWorkAgreement? Find(string code)
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

    public void Save(StatementOfWorkAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StatementOfWorkAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects statement of work agreements. Empty fields match all.</summary>
public sealed record StatementOfWorkAgreementQuery(string Region, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(StatementOfWorkAgreement item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StatementOfWorkAgreement> Apply(List<StatementOfWorkAgreement> items)
    {
        var matched = Collections.Filter(items, (StatementOfWorkAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of statement of work agreement store.</summary>
public static class StatementOfWorkAgreementStore
{
    public static IStatementOfWorkAgreementStore NewMemoryStatementOfWorkAgreementStore()
    {
        return new MemoryStatementOfWorkAgreementStore(
            new Dictionary<string, StatementOfWorkAgreement>(),
            new List<string>());
    }

    public static IStatementOfWorkAgreementStore NewCachedStatementOfWorkAgreementStore(MemoryStatementOfWorkAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedStatementOfWorkAgreementStore(
            inner,
            StatementOfWorkAgreementCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<StatementOfWorkAgreement> FindStatementOfWorkAgreements(IStatementOfWorkAgreementStore store, StatementOfWorkAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
