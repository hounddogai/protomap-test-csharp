namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool ledgers by code.</summary>
public interface IBonusPoolLedgerStore
{
    BonusPoolLedger? Find(string code);
    void Save(BonusPoolLedger item);
    bool Remove(string code);
    List<BonusPoolLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolLedgerStore(Dictionary<string, BonusPoolLedger> items, List<string> order) : IBonusPoolLedgerStore
{
    public Dictionary<string, BonusPoolLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolLedger item)
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

    public List<BonusPoolLedger> List()
    {
        var outValue = new List<BonusPoolLedger>();
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
public sealed class CachedBonusPoolLedgerStore(MemoryBonusPoolLedgerStore inner, BonusPoolLedgerCache cache, int hits, int misses) : IBonusPoolLedgerStore
{
    public MemoryBonusPoolLedgerStore Inner { get; set; } = inner;
    public BonusPoolLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolLedger? Find(string code)
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

    public void Save(BonusPoolLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool ledgers. Empty fields match all.</summary>
public sealed record BonusPoolLedgerQuery(string Segment, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolLedger item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<BonusPoolLedger> Apply(List<BonusPoolLedger> items)
    {
        var matched = Collections.Filter(items, (BonusPoolLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool ledger store.</summary>
public static class BonusPoolLedgerStore
{
    public static IBonusPoolLedgerStore NewMemoryBonusPoolLedgerStore()
    {
        return new MemoryBonusPoolLedgerStore(new Dictionary<string, BonusPoolLedger>(), new List<string>());
    }

    public static IBonusPoolLedgerStore NewCachedBonusPoolLedgerStore(MemoryBonusPoolLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolLedgerStore(inner, BonusPoolLedgerCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BonusPoolLedger> FindBonusPoolLedgers(IBonusPoolLedgerStore store, BonusPoolLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
