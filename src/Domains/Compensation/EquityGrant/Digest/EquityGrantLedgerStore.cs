namespace Acme.Workforce.Compensation.EquityGrant.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant ledgers by code.</summary>
public interface IEquityGrantLedgerStore
{
    EquityGrantLedger? Find(string code);
    void Save(EquityGrantLedger item);
    bool Remove(string code);
    List<EquityGrantLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantLedgerStore(Dictionary<string, EquityGrantLedger> items, List<string> order) : IEquityGrantLedgerStore
{
    public Dictionary<string, EquityGrantLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantLedger item)
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

    public List<EquityGrantLedger> List()
    {
        var outValue = new List<EquityGrantLedger>();
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
public sealed class CachedEquityGrantLedgerStore(MemoryEquityGrantLedgerStore inner, EquityGrantLedgerCache cache, int hits, int misses) : IEquityGrantLedgerStore
{
    public MemoryEquityGrantLedgerStore Inner { get; set; } = inner;
    public EquityGrantLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantLedger? Find(string code)
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

    public void Save(EquityGrantLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant ledgers. Empty fields match all.</summary>
public sealed record EquityGrantLedgerQuery(string Locale, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantLedger item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantLedger> Apply(List<EquityGrantLedger> items)
    {
        var matched = Collections.Filter(items, (EquityGrantLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant ledger store.</summary>
public static class EquityGrantLedgerStore
{
    public static IEquityGrantLedgerStore NewMemoryEquityGrantLedgerStore()
    {
        return new MemoryEquityGrantLedgerStore(new Dictionary<string, EquityGrantLedger>(), new List<string>());
    }

    public static IEquityGrantLedgerStore NewCachedEquityGrantLedgerStore(MemoryEquityGrantLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantLedgerStore(inner, EquityGrantLedgerCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<EquityGrantLedger> FindEquityGrantLedgers(IEquityGrantLedgerStore store, EquityGrantLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
