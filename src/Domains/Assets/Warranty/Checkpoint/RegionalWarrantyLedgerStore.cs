namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional warranty ledgers by code.</summary>
public interface IRegionalWarrantyLedgerStore
{
    RegionalWarrantyLedger? Find(string code);
    void Save(RegionalWarrantyLedger item);
    bool Remove(string code);
    List<RegionalWarrantyLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalWarrantyLedgerStore(Dictionary<string, RegionalWarrantyLedger> items, List<string> order) : IRegionalWarrantyLedgerStore
{
    public Dictionary<string, RegionalWarrantyLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalWarrantyLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalWarrantyLedger item)
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

    public List<RegionalWarrantyLedger> List()
    {
        var outValue = new List<RegionalWarrantyLedger>();
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
public sealed class CachedRegionalWarrantyLedgerStore(MemoryRegionalWarrantyLedgerStore inner, RegionalWarrantyLedgerCache cache, int hits, int misses) : IRegionalWarrantyLedgerStore
{
    public MemoryRegionalWarrantyLedgerStore Inner { get; set; } = inner;
    public RegionalWarrantyLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalWarrantyLedger? Find(string code)
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

    public void Save(RegionalWarrantyLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalWarrantyLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional warranty ledgers. Empty fields match all.</summary>
public sealed record RegionalWarrantyLedgerQuery(string Country, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalWarrantyLedger item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalWarrantyLedger> Apply(List<RegionalWarrantyLedger> items)
    {
        var matched = Collections.Filter(items, (RegionalWarrantyLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional warranty ledger store.</summary>
public static class RegionalWarrantyLedgerStore
{
    public static IRegionalWarrantyLedgerStore NewMemoryRegionalWarrantyLedgerStore()
    {
        return new MemoryRegionalWarrantyLedgerStore(
            new Dictionary<string, RegionalWarrantyLedger>(),
            new List<string>());
    }

    public static IRegionalWarrantyLedgerStore NewCachedRegionalWarrantyLedgerStore(MemoryRegionalWarrantyLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalWarrantyLedgerStore(
            inner,
            RegionalWarrantyLedgerCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalWarrantyLedger> FindRegionalWarrantyLedgers(IRegionalWarrantyLedgerStore store, RegionalWarrantyLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
