namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback license seat ledgers by code.</summary>
public interface IFallbackLicenseSeatLedgerStore
{
    FallbackLicenseSeatLedger? Find(string code);
    void Save(FallbackLicenseSeatLedger item);
    bool Remove(string code);
    List<FallbackLicenseSeatLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackLicenseSeatLedgerStore(Dictionary<string, FallbackLicenseSeatLedger> items, List<string> order) : IFallbackLicenseSeatLedgerStore
{
    public Dictionary<string, FallbackLicenseSeatLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackLicenseSeatLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackLicenseSeatLedger item)
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

    public List<FallbackLicenseSeatLedger> List()
    {
        var outValue = new List<FallbackLicenseSeatLedger>();
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
public sealed class CachedFallbackLicenseSeatLedgerStore(MemoryFallbackLicenseSeatLedgerStore inner, FallbackLicenseSeatLedgerCache cache, int hits, int misses) : IFallbackLicenseSeatLedgerStore
{
    public MemoryFallbackLicenseSeatLedgerStore Inner { get; set; } = inner;
    public FallbackLicenseSeatLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackLicenseSeatLedger? Find(string code)
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

    public void Save(FallbackLicenseSeatLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackLicenseSeatLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback license seat ledgers. Empty fields match all.</summary>
public sealed record FallbackLicenseSeatLedgerQuery(string Segment, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackLicenseSeatLedger item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackLicenseSeatLedger> Apply(List<FallbackLicenseSeatLedger> items)
    {
        var matched = Collections.Filter(items, (FallbackLicenseSeatLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback license seat ledger store.</summary>
public static class FallbackLicenseSeatLedgerStore
{
    public static IFallbackLicenseSeatLedgerStore NewMemoryFallbackLicenseSeatLedgerStore()
    {
        return new MemoryFallbackLicenseSeatLedgerStore(
            new Dictionary<string, FallbackLicenseSeatLedger>(),
            new List<string>());
    }

    public static IFallbackLicenseSeatLedgerStore NewCachedFallbackLicenseSeatLedgerStore(MemoryFallbackLicenseSeatLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackLicenseSeatLedgerStore(
            inner,
            FallbackLicenseSeatLedgerCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackLicenseSeatLedger> FindFallbackLicenseSeatLedgers(IFallbackLicenseSeatLedgerStore store, FallbackLicenseSeatLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
