namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg ledgers by code.</summary>
public interface IReorgLedgerStore
{
    ReorgLedger? Find(string code);
    void Save(ReorgLedger item);
    bool Remove(string code);
    List<ReorgLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgLedgerStore(Dictionary<string, ReorgLedger> items, List<string> order) : IReorgLedgerStore
{
    public Dictionary<string, ReorgLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgLedger item)
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

    public List<ReorgLedger> List()
    {
        var outValue = new List<ReorgLedger>();
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
public sealed class CachedReorgLedgerStore(MemoryReorgLedgerStore inner, ReorgLedgerCache cache, int hits, int misses) : IReorgLedgerStore
{
    public MemoryReorgLedgerStore Inner { get; set; } = inner;
    public ReorgLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgLedger? Find(string code)
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

    public void Save(ReorgLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg ledgers. Empty fields match all.</summary>
public sealed record ReorgLedgerQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgLedger item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgLedger> Apply(List<ReorgLedger> items)
    {
        var matched = Collections.Filter(items, (ReorgLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg ledger store.</summary>
public static class ReorgLedgerStore
{
    public static IReorgLedgerStore NewMemoryReorgLedgerStore()
    {
        return new MemoryReorgLedgerStore(new Dictionary<string, ReorgLedger>(), new List<string>());
    }

    public static IReorgLedgerStore NewCachedReorgLedgerStore(MemoryReorgLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedReorgLedgerStore(inner, ReorgLedgerCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ReorgLedger> FindReorgLedgers(IReorgLedgerStore store, ReorgLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
