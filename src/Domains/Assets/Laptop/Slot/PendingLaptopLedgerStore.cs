namespace Acme.Workforce.Assets.Laptop.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending laptop ledgers by code.</summary>
public interface IPendingLaptopLedgerStore
{
    PendingLaptopLedger? Find(string code);
    void Save(PendingLaptopLedger item);
    bool Remove(string code);
    List<PendingLaptopLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingLaptopLedgerStore(Dictionary<string, PendingLaptopLedger> items, List<string> order) : IPendingLaptopLedgerStore
{
    public Dictionary<string, PendingLaptopLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingLaptopLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingLaptopLedger item)
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

    public List<PendingLaptopLedger> List()
    {
        var outValue = new List<PendingLaptopLedger>();
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
public sealed class CachedPendingLaptopLedgerStore(MemoryPendingLaptopLedgerStore inner, PendingLaptopLedgerCache cache, int hits, int misses) : IPendingLaptopLedgerStore
{
    public MemoryPendingLaptopLedgerStore Inner { get; set; } = inner;
    public PendingLaptopLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingLaptopLedger? Find(string code)
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

    public void Save(PendingLaptopLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingLaptopLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending laptop ledgers. Empty fields match all.</summary>
public sealed record PendingLaptopLedgerQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PendingLaptopLedger item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingLaptopLedger> Apply(List<PendingLaptopLedger> items)
    {
        var matched = Collections.Filter(items, (PendingLaptopLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending laptop ledger store.</summary>
public static class PendingLaptopLedgerStore
{
    public static IPendingLaptopLedgerStore NewMemoryPendingLaptopLedgerStore()
    {
        return new MemoryPendingLaptopLedgerStore(new Dictionary<string, PendingLaptopLedger>(), new List<string>());
    }

    public static IPendingLaptopLedgerStore NewCachedPendingLaptopLedgerStore(MemoryPendingLaptopLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedPendingLaptopLedgerStore(inner, PendingLaptopLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingLaptopLedger> FindPendingLaptopLedgers(IPendingLaptopLedgerStore store, PendingLaptopLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
