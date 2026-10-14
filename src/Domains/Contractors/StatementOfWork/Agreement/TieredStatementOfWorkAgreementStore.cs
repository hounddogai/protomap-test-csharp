namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered statement of work agreements by code.</summary>
public interface ITieredStatementOfWorkAgreementStore
{
    TieredStatementOfWorkAgreement? Find(string code);
    void Save(TieredStatementOfWorkAgreement item);
    bool Remove(string code);
    List<TieredStatementOfWorkAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredStatementOfWorkAgreementStore(Dictionary<string, TieredStatementOfWorkAgreement> items, List<string> order) : ITieredStatementOfWorkAgreementStore
{
    public Dictionary<string, TieredStatementOfWorkAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredStatementOfWorkAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredStatementOfWorkAgreement item)
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

    public List<TieredStatementOfWorkAgreement> List()
    {
        var outValue = new List<TieredStatementOfWorkAgreement>();
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
public sealed class CachedTieredStatementOfWorkAgreementStore(MemoryTieredStatementOfWorkAgreementStore inner, TieredStatementOfWorkAgreementCache cache, int hits, int misses) : ITieredStatementOfWorkAgreementStore
{
    public MemoryTieredStatementOfWorkAgreementStore Inner { get; set; } = inner;
    public TieredStatementOfWorkAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredStatementOfWorkAgreement? Find(string code)
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

    public void Save(TieredStatementOfWorkAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredStatementOfWorkAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered statement of work agreements. Empty fields match all.</summary>
public sealed record TieredStatementOfWorkAgreementQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(TieredStatementOfWorkAgreement item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<TieredStatementOfWorkAgreement> Apply(List<TieredStatementOfWorkAgreement> items)
    {
        var matched = Collections.Filter(items, (TieredStatementOfWorkAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered statement of work agreement store.</summary>
public static class TieredStatementOfWorkAgreementStore
{
    public static ITieredStatementOfWorkAgreementStore NewMemoryTieredStatementOfWorkAgreementStore()
    {
        return new MemoryTieredStatementOfWorkAgreementStore(
            new Dictionary<string, TieredStatementOfWorkAgreement>(),
            new List<string>());
    }

    public static ITieredStatementOfWorkAgreementStore NewCachedTieredStatementOfWorkAgreementStore(MemoryTieredStatementOfWorkAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedTieredStatementOfWorkAgreementStore(
            inner,
            TieredStatementOfWorkAgreementCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<TieredStatementOfWorkAgreement> FindTieredStatementOfWorkAgreements(ITieredStatementOfWorkAgreementStore store, TieredStatementOfWorkAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
