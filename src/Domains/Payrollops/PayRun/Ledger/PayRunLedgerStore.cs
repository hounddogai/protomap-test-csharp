namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run ledgers by code.</summary>
public interface IPayRunLedgerStore
{
    PayRunLedger? Find(string code);
    void Save(PayRunLedger item);
    bool Remove(string code);
    List<PayRunLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunLedgerStore(Dictionary<string, PayRunLedger> items, List<string> order) : IPayRunLedgerStore
{
    public Dictionary<string, PayRunLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunLedger item)
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

    public List<PayRunLedger> List()
    {
        var outValue = new List<PayRunLedger>();
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
public sealed class CachedPayRunLedgerStore(MemoryPayRunLedgerStore inner, PayRunLedgerCache cache, int hits, int misses) : IPayRunLedgerStore
{
    public MemoryPayRunLedgerStore Inner { get; set; } = inner;
    public PayRunLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunLedger? Find(string code)
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

    public void Save(PayRunLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run ledgers. Empty fields match all.</summary>
public sealed record PayRunLedgerQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunLedger item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunLedger> Apply(List<PayRunLedger> items)
    {
        var matched = Collections.Filter(items, (PayRunLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run ledger store.</summary>
public static class PayRunLedgerStore
{
    public static IPayRunLedgerStore NewMemoryPayRunLedgerStore()
    {
        return new MemoryPayRunLedgerStore(new Dictionary<string, PayRunLedger>(), new List<string>());
    }

    public static IPayRunLedgerStore NewCachedPayRunLedgerStore(MemoryPayRunLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunLedgerStore(inner, PayRunLedgerCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayRunLedger> FindPayRunLedgers(IPayRunLedgerStore store, PayRunLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
