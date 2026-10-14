namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy statement of work agreements by code.</summary>
public interface ILegacyStatementOfWorkAgreementStore
{
    LegacyStatementOfWorkAgreement? Find(string code);
    void Save(LegacyStatementOfWorkAgreement item);
    bool Remove(string code);
    List<LegacyStatementOfWorkAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyStatementOfWorkAgreementStore(Dictionary<string, LegacyStatementOfWorkAgreement> items, List<string> order) : ILegacyStatementOfWorkAgreementStore
{
    public Dictionary<string, LegacyStatementOfWorkAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyStatementOfWorkAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyStatementOfWorkAgreement item)
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

    public List<LegacyStatementOfWorkAgreement> List()
    {
        var outValue = new List<LegacyStatementOfWorkAgreement>();
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
public sealed class CachedLegacyStatementOfWorkAgreementStore(MemoryLegacyStatementOfWorkAgreementStore inner, LegacyStatementOfWorkAgreementCache cache, int hits, int misses) : ILegacyStatementOfWorkAgreementStore
{
    public MemoryLegacyStatementOfWorkAgreementStore Inner { get; set; } = inner;
    public LegacyStatementOfWorkAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyStatementOfWorkAgreement? Find(string code)
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

    public void Save(LegacyStatementOfWorkAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyStatementOfWorkAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy statement of work agreements. Empty fields match all.</summary>
public sealed record LegacyStatementOfWorkAgreementQuery(string Country, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyStatementOfWorkAgreement item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyStatementOfWorkAgreement> Apply(List<LegacyStatementOfWorkAgreement> items)
    {
        var matched = Collections.Filter(items, (LegacyStatementOfWorkAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy statement of work agreement store.</summary>
public static class LegacyStatementOfWorkAgreementStore
{
    public static ILegacyStatementOfWorkAgreementStore NewMemoryLegacyStatementOfWorkAgreementStore()
    {
        return new MemoryLegacyStatementOfWorkAgreementStore(
            new Dictionary<string, LegacyStatementOfWorkAgreement>(),
            new List<string>());
    }

    public static ILegacyStatementOfWorkAgreementStore NewCachedLegacyStatementOfWorkAgreementStore(MemoryLegacyStatementOfWorkAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyStatementOfWorkAgreementStore(
            inner,
            LegacyStatementOfWorkAgreementCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LegacyStatementOfWorkAgreement> FindLegacyStatementOfWorkAgreements(ILegacyStatementOfWorkAgreementStore store, LegacyStatementOfWorkAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
