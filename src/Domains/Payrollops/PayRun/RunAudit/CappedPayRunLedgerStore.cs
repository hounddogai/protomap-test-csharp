namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped pay run ledgers by code.</summary>
public interface ICappedPayRunLedgerStore
{
    CappedPayRunLedger? Find(string code);
    void Save(CappedPayRunLedger item);
    bool Remove(string code);
    List<CappedPayRunLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPayRunLedgerStore(Dictionary<string, CappedPayRunLedger> items, List<string> order) : ICappedPayRunLedgerStore
{
    public Dictionary<string, CappedPayRunLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPayRunLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPayRunLedger item)
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

    public List<CappedPayRunLedger> List()
    {
        var outValue = new List<CappedPayRunLedger>();
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
public sealed class CachedCappedPayRunLedgerStore(MemoryCappedPayRunLedgerStore inner, CappedPayRunLedgerCache cache, int hits, int misses) : ICappedPayRunLedgerStore
{
    public MemoryCappedPayRunLedgerStore Inner { get; set; } = inner;
    public CappedPayRunLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPayRunLedger? Find(string code)
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

    public void Save(CappedPayRunLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPayRunLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped pay run ledgers. Empty fields match all.</summary>
public sealed record CappedPayRunLedgerQuery(string Channel, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPayRunLedger item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedPayRunLedger> Apply(List<CappedPayRunLedger> items)
    {
        var matched = Collections.Filter(items, (CappedPayRunLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped pay run ledger store.</summary>
public static class CappedPayRunLedgerStore
{
    public static ICappedPayRunLedgerStore NewMemoryCappedPayRunLedgerStore()
    {
        return new MemoryCappedPayRunLedgerStore(new Dictionary<string, CappedPayRunLedger>(), new List<string>());
    }

    public static ICappedPayRunLedgerStore NewCachedCappedPayRunLedgerStore(MemoryCappedPayRunLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPayRunLedgerStore(inner, CappedPayRunLedgerCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CappedPayRunLedger> FindCappedPayRunLedgers(ICappedPayRunLedgerStore store, CappedPayRunLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
