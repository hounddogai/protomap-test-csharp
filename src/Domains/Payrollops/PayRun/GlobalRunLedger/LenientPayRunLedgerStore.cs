namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient pay run ledgers by code.</summary>
public interface ILenientPayRunLedgerStore
{
    LenientPayRunLedger? Find(string code);
    void Save(LenientPayRunLedger item);
    bool Remove(string code);
    List<LenientPayRunLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPayRunLedgerStore(Dictionary<string, LenientPayRunLedger> items, List<string> order) : ILenientPayRunLedgerStore
{
    public Dictionary<string, LenientPayRunLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPayRunLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPayRunLedger item)
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

    public List<LenientPayRunLedger> List()
    {
        var outValue = new List<LenientPayRunLedger>();
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
public sealed class CachedLenientPayRunLedgerStore(MemoryLenientPayRunLedgerStore inner, LenientPayRunLedgerCache cache, int hits, int misses) : ILenientPayRunLedgerStore
{
    public MemoryLenientPayRunLedgerStore Inner { get; set; } = inner;
    public LenientPayRunLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPayRunLedger? Find(string code)
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

    public void Save(LenientPayRunLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPayRunLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient pay run ledgers. Empty fields match all.</summary>
public sealed record LenientPayRunLedgerQuery(string Name, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPayRunLedger item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<LenientPayRunLedger> Apply(List<LenientPayRunLedger> items)
    {
        var matched = Collections.Filter(items, (LenientPayRunLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient pay run ledger store.</summary>
public static class LenientPayRunLedgerStore
{
    public static ILenientPayRunLedgerStore NewMemoryLenientPayRunLedgerStore()
    {
        return new MemoryLenientPayRunLedgerStore(new Dictionary<string, LenientPayRunLedger>(), new List<string>());
    }

    public static ILenientPayRunLedgerStore NewCachedLenientPayRunLedgerStore(MemoryLenientPayRunLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPayRunLedgerStore(inner, LenientPayRunLedgerCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LenientPayRunLedger> FindLenientPayRunLedgers(ILenientPayRunLedgerStore store, LenientPayRunLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
