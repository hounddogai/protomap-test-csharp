namespace Acme.Workforce.Assets.LicenseSeat.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps license seat ledgers by code.</summary>
public interface ILicenseSeatLedgerStore
{
    LicenseSeatLedger? Find(string code);
    void Save(LicenseSeatLedger item);
    bool Remove(string code);
    List<LicenseSeatLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLicenseSeatLedgerStore(Dictionary<string, LicenseSeatLedger> items, List<string> order) : ILicenseSeatLedgerStore
{
    public Dictionary<string, LicenseSeatLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LicenseSeatLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LicenseSeatLedger item)
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

    public List<LicenseSeatLedger> List()
    {
        var outValue = new List<LicenseSeatLedger>();
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
public sealed class CachedLicenseSeatLedgerStore(MemoryLicenseSeatLedgerStore inner, LicenseSeatLedgerCache cache, int hits, int misses) : ILicenseSeatLedgerStore
{
    public MemoryLicenseSeatLedgerStore Inner { get; set; } = inner;
    public LicenseSeatLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LicenseSeatLedger? Find(string code)
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

    public void Save(LicenseSeatLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LicenseSeatLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects license seat ledgers. Empty fields match all.</summary>
public sealed record LicenseSeatLedgerQuery(string Label, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LicenseSeatLedger item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LicenseSeatLedger> Apply(List<LicenseSeatLedger> items)
    {
        var matched = Collections.Filter(items, (LicenseSeatLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of license seat ledger store.</summary>
public static class LicenseSeatLedgerStore
{
    public static ILicenseSeatLedgerStore NewMemoryLicenseSeatLedgerStore()
    {
        return new MemoryLicenseSeatLedgerStore(new Dictionary<string, LicenseSeatLedger>(), new List<string>());
    }

    public static ILicenseSeatLedgerStore NewCachedLicenseSeatLedgerStore(MemoryLicenseSeatLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedLicenseSeatLedgerStore(inner, LicenseSeatLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LicenseSeatLedger> FindLicenseSeatLedgers(ILicenseSeatLedgerStore store, LicenseSeatLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
