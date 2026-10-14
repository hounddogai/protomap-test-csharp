namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient equity grant ledgers by code.</summary>
public interface ILenientEquityGrantLedgerStore
{
    LenientEquityGrantLedger? Find(string code);
    void Save(LenientEquityGrantLedger item);
    bool Remove(string code);
    List<LenientEquityGrantLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientEquityGrantLedgerStore(Dictionary<string, LenientEquityGrantLedger> items, List<string> order) : ILenientEquityGrantLedgerStore
{
    public Dictionary<string, LenientEquityGrantLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientEquityGrantLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientEquityGrantLedger item)
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

    public List<LenientEquityGrantLedger> List()
    {
        var outValue = new List<LenientEquityGrantLedger>();
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
public sealed class CachedLenientEquityGrantLedgerStore(MemoryLenientEquityGrantLedgerStore inner, LenientEquityGrantLedgerCache cache, int hits, int misses) : ILenientEquityGrantLedgerStore
{
    public MemoryLenientEquityGrantLedgerStore Inner { get; set; } = inner;
    public LenientEquityGrantLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientEquityGrantLedger? Find(string code)
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

    public void Save(LenientEquityGrantLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientEquityGrantLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient equity grant ledgers. Empty fields match all.</summary>
public sealed record LenientEquityGrantLedgerQuery(string Title, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientEquityGrantLedger item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientEquityGrantLedger> Apply(List<LenientEquityGrantLedger> items)
    {
        var matched = Collections.Filter(items, (LenientEquityGrantLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient equity grant ledger store.</summary>
public static class LenientEquityGrantLedgerStore
{
    public static ILenientEquityGrantLedgerStore NewMemoryLenientEquityGrantLedgerStore()
    {
        return new MemoryLenientEquityGrantLedgerStore(
            new Dictionary<string, LenientEquityGrantLedger>(),
            new List<string>());
    }

    public static ILenientEquityGrantLedgerStore NewCachedLenientEquityGrantLedgerStore(MemoryLenientEquityGrantLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedLenientEquityGrantLedgerStore(
            inner,
            LenientEquityGrantLedgerCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientEquityGrantLedger> FindLenientEquityGrantLedgers(ILenientEquityGrantLedgerStore store, LenientEquityGrantLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
