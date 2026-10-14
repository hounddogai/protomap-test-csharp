namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered accrual ledgers by code.</summary>
public interface ITieredAccrualLedgerStore
{
    TieredAccrualLedger? Find(string code);
    void Save(TieredAccrualLedger item);
    bool Remove(string code);
    List<TieredAccrualLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredAccrualLedgerStore(Dictionary<string, TieredAccrualLedger> items, List<string> order) : ITieredAccrualLedgerStore
{
    public Dictionary<string, TieredAccrualLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredAccrualLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredAccrualLedger item)
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

    public List<TieredAccrualLedger> List()
    {
        var outValue = new List<TieredAccrualLedger>();
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
public sealed class CachedTieredAccrualLedgerStore(MemoryTieredAccrualLedgerStore inner, TieredAccrualLedgerCache cache, int hits, int misses) : ITieredAccrualLedgerStore
{
    public MemoryTieredAccrualLedgerStore Inner { get; set; } = inner;
    public TieredAccrualLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredAccrualLedger? Find(string code)
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

    public void Save(TieredAccrualLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredAccrualLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered accrual ledgers. Empty fields match all.</summary>
public sealed record TieredAccrualLedgerQuery(string Locale, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(TieredAccrualLedger item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredAccrualLedger> Apply(List<TieredAccrualLedger> items)
    {
        var matched = Collections.Filter(items, (TieredAccrualLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered accrual ledger store.</summary>
public static class TieredAccrualLedgerStore
{
    public static ITieredAccrualLedgerStore NewMemoryTieredAccrualLedgerStore()
    {
        return new MemoryTieredAccrualLedgerStore(new Dictionary<string, TieredAccrualLedger>(), new List<string>());
    }

    public static ITieredAccrualLedgerStore NewCachedTieredAccrualLedgerStore(MemoryTieredAccrualLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedTieredAccrualLedgerStore(inner, TieredAccrualLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TieredAccrualLedger> FindTieredAccrualLedgers(ITieredAccrualLedgerStore store, TieredAccrualLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
