namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered pay run ledgers by code.</summary>
public interface ITieredPayRunLedgerStore
{
    TieredPayRunLedger? Find(string code);
    void Save(TieredPayRunLedger item);
    bool Remove(string code);
    List<TieredPayRunLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPayRunLedgerStore(Dictionary<string, TieredPayRunLedger> items, List<string> order) : ITieredPayRunLedgerStore
{
    public Dictionary<string, TieredPayRunLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPayRunLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPayRunLedger item)
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

    public List<TieredPayRunLedger> List()
    {
        var outValue = new List<TieredPayRunLedger>();
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
public sealed class CachedTieredPayRunLedgerStore(MemoryTieredPayRunLedgerStore inner, TieredPayRunLedgerCache cache, int hits, int misses) : ITieredPayRunLedgerStore
{
    public MemoryTieredPayRunLedgerStore Inner { get; set; } = inner;
    public TieredPayRunLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPayRunLedger? Find(string code)
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

    public void Save(TieredPayRunLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPayRunLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered pay run ledgers. Empty fields match all.</summary>
public sealed record TieredPayRunLedgerQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPayRunLedger item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredPayRunLedger> Apply(List<TieredPayRunLedger> items)
    {
        var matched = Collections.Filter(items, (TieredPayRunLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered pay run ledger store.</summary>
public static class TieredPayRunLedgerStore
{
    public static ITieredPayRunLedgerStore NewMemoryTieredPayRunLedgerStore()
    {
        return new MemoryTieredPayRunLedgerStore(new Dictionary<string, TieredPayRunLedger>(), new List<string>());
    }

    public static ITieredPayRunLedgerStore NewCachedTieredPayRunLedgerStore(MemoryTieredPayRunLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPayRunLedgerStore(inner, TieredPayRunLedgerCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredPayRunLedger> FindTieredPayRunLedgers(ITieredPayRunLedgerStore store, TieredPayRunLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
