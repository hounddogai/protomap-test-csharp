namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default pay run ledgers by code.</summary>
public interface IDefaultPayRunLedgerStore
{
    DefaultPayRunLedger? Find(string code);
    void Save(DefaultPayRunLedger item);
    bool Remove(string code);
    List<DefaultPayRunLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPayRunLedgerStore(Dictionary<string, DefaultPayRunLedger> items, List<string> order) : IDefaultPayRunLedgerStore
{
    public Dictionary<string, DefaultPayRunLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPayRunLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPayRunLedger item)
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

    public List<DefaultPayRunLedger> List()
    {
        var outValue = new List<DefaultPayRunLedger>();
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
public sealed class CachedDefaultPayRunLedgerStore(MemoryDefaultPayRunLedgerStore inner, DefaultPayRunLedgerCache cache, int hits, int misses) : IDefaultPayRunLedgerStore
{
    public MemoryDefaultPayRunLedgerStore Inner { get; set; } = inner;
    public DefaultPayRunLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPayRunLedger? Find(string code)
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

    public void Save(DefaultPayRunLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPayRunLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default pay run ledgers. Empty fields match all.</summary>
public sealed record DefaultPayRunLedgerQuery(string Source, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPayRunLedger item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<DefaultPayRunLedger> Apply(List<DefaultPayRunLedger> items)
    {
        var matched = Collections.Filter(items, (DefaultPayRunLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default pay run ledger store.</summary>
public static class DefaultPayRunLedgerStore
{
    public static IDefaultPayRunLedgerStore NewMemoryDefaultPayRunLedgerStore()
    {
        return new MemoryDefaultPayRunLedgerStore(new Dictionary<string, DefaultPayRunLedger>(), new List<string>());
    }

    public static IDefaultPayRunLedgerStore NewCachedDefaultPayRunLedgerStore(MemoryDefaultPayRunLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPayRunLedgerStore(inner, DefaultPayRunLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DefaultPayRunLedger> FindDefaultPayRunLedgers(IDefaultPayRunLedgerStore store, DefaultPayRunLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
