namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional access badge ledgers by code.</summary>
public interface IRegionalAccessBadgeLedgerStore
{
    RegionalAccessBadgeLedger? Find(string code);
    void Save(RegionalAccessBadgeLedger item);
    bool Remove(string code);
    List<RegionalAccessBadgeLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalAccessBadgeLedgerStore(Dictionary<string, RegionalAccessBadgeLedger> items, List<string> order) : IRegionalAccessBadgeLedgerStore
{
    public Dictionary<string, RegionalAccessBadgeLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalAccessBadgeLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalAccessBadgeLedger item)
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

    public List<RegionalAccessBadgeLedger> List()
    {
        var outValue = new List<RegionalAccessBadgeLedger>();
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
public sealed class CachedRegionalAccessBadgeLedgerStore(MemoryRegionalAccessBadgeLedgerStore inner, RegionalAccessBadgeLedgerCache cache, int hits, int misses) : IRegionalAccessBadgeLedgerStore
{
    public MemoryRegionalAccessBadgeLedgerStore Inner { get; set; } = inner;
    public RegionalAccessBadgeLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalAccessBadgeLedger? Find(string code)
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

    public void Save(RegionalAccessBadgeLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalAccessBadgeLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional access badge ledgers. Empty fields match all.</summary>
public sealed record RegionalAccessBadgeLedgerQuery(string Source, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalAccessBadgeLedger item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalAccessBadgeLedger> Apply(List<RegionalAccessBadgeLedger> items)
    {
        var matched = Collections.Filter(items, (RegionalAccessBadgeLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional access badge ledger store.</summary>
public static class RegionalAccessBadgeLedgerStore
{
    public static IRegionalAccessBadgeLedgerStore NewMemoryRegionalAccessBadgeLedgerStore()
    {
        return new MemoryRegionalAccessBadgeLedgerStore(
            new Dictionary<string, RegionalAccessBadgeLedger>(),
            new List<string>());
    }

    public static IRegionalAccessBadgeLedgerStore NewCachedRegionalAccessBadgeLedgerStore(MemoryRegionalAccessBadgeLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalAccessBadgeLedgerStore(
            inner,
            RegionalAccessBadgeLedgerCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalAccessBadgeLedger> FindRegionalAccessBadgeLedgers(IRegionalAccessBadgeLedgerStore store, RegionalAccessBadgeLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
