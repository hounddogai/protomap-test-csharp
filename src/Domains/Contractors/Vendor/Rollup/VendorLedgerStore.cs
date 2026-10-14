namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor ledgers by code.</summary>
public interface IVendorLedgerStore
{
    VendorLedger? Find(string code);
    void Save(VendorLedger item);
    bool Remove(string code);
    List<VendorLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorLedgerStore(Dictionary<string, VendorLedger> items, List<string> order) : IVendorLedgerStore
{
    public Dictionary<string, VendorLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorLedger item)
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

    public List<VendorLedger> List()
    {
        var outValue = new List<VendorLedger>();
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
public sealed class CachedVendorLedgerStore(MemoryVendorLedgerStore inner, VendorLedgerCache cache, int hits, int misses) : IVendorLedgerStore
{
    public MemoryVendorLedgerStore Inner { get; set; } = inner;
    public VendorLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorLedger? Find(string code)
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

    public void Save(VendorLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor ledgers. Empty fields match all.</summary>
public sealed record VendorLedgerQuery(string Team, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(VendorLedger item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorLedger> Apply(List<VendorLedger> items)
    {
        var matched = Collections.Filter(items, (VendorLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor ledger store.</summary>
public static class VendorLedgerStore
{
    public static IVendorLedgerStore NewMemoryVendorLedgerStore()
    {
        return new MemoryVendorLedgerStore(new Dictionary<string, VendorLedger>(), new List<string>());
    }

    public static IVendorLedgerStore NewCachedVendorLedgerStore(MemoryVendorLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedVendorLedgerStore(inner, VendorLedgerCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<VendorLedger> FindVendorLedgers(IVendorLedgerStore store, VendorLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
