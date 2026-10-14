namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination ledgers by code.</summary>
public interface INominationLedgerStore
{
    NominationLedger? Find(string code);
    void Save(NominationLedger item);
    bool Remove(string code);
    List<NominationLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationLedgerStore(Dictionary<string, NominationLedger> items, List<string> order) : INominationLedgerStore
{
    public Dictionary<string, NominationLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationLedger item)
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

    public List<NominationLedger> List()
    {
        var outValue = new List<NominationLedger>();
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
public sealed class CachedNominationLedgerStore(MemoryNominationLedgerStore inner, NominationLedgerCache cache, int hits, int misses) : INominationLedgerStore
{
    public MemoryNominationLedgerStore Inner { get; set; } = inner;
    public NominationLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationLedger? Find(string code)
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

    public void Save(NominationLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination ledgers. Empty fields match all.</summary>
public sealed record NominationLedgerQuery(string Department, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(NominationLedger item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<NominationLedger> Apply(List<NominationLedger> items)
    {
        var matched = Collections.Filter(items, (NominationLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination ledger store.</summary>
public static class NominationLedgerStore
{
    public static INominationLedgerStore NewMemoryNominationLedgerStore()
    {
        return new MemoryNominationLedgerStore(new Dictionary<string, NominationLedger>(), new List<string>());
    }

    public static INominationLedgerStore NewCachedNominationLedgerStore(MemoryNominationLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedNominationLedgerStore(inner, NominationLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<NominationLedger> FindNominationLedgers(INominationLedgerStore store, NominationLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
