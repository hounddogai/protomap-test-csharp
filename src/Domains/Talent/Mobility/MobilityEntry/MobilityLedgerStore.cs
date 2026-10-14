namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility ledgers by code.</summary>
public interface IMobilityLedgerStore
{
    MobilityLedger? Find(string code);
    void Save(MobilityLedger item);
    bool Remove(string code);
    List<MobilityLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityLedgerStore(Dictionary<string, MobilityLedger> items, List<string> order) : IMobilityLedgerStore
{
    public Dictionary<string, MobilityLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityLedger item)
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

    public List<MobilityLedger> List()
    {
        var outValue = new List<MobilityLedger>();
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
public sealed class CachedMobilityLedgerStore(MemoryMobilityLedgerStore inner, MobilityLedgerCache cache, int hits, int misses) : IMobilityLedgerStore
{
    public MemoryMobilityLedgerStore Inner { get; set; } = inner;
    public MobilityLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityLedger? Find(string code)
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

    public void Save(MobilityLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility ledgers. Empty fields match all.</summary>
public sealed record MobilityLedgerQuery(string Locale, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityLedger item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityLedger> Apply(List<MobilityLedger> items)
    {
        var matched = Collections.Filter(items, (MobilityLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility ledger store.</summary>
public static class MobilityLedgerStore
{
    public static IMobilityLedgerStore NewMemoryMobilityLedgerStore()
    {
        return new MemoryMobilityLedgerStore(new Dictionary<string, MobilityLedger>(), new List<string>());
    }

    public static IMobilityLedgerStore NewCachedMobilityLedgerStore(MemoryMobilityLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityLedgerStore(inner, MobilityLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<MobilityLedger> FindMobilityLedgers(IMobilityLedgerStore store, MobilityLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
