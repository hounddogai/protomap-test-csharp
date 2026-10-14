namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback desk ledgers by code.</summary>
public interface IFallbackDeskLedgerStore
{
    FallbackDeskLedger? Find(string code);
    void Save(FallbackDeskLedger item);
    bool Remove(string code);
    List<FallbackDeskLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackDeskLedgerStore(Dictionary<string, FallbackDeskLedger> items, List<string> order) : IFallbackDeskLedgerStore
{
    public Dictionary<string, FallbackDeskLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackDeskLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackDeskLedger item)
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

    public List<FallbackDeskLedger> List()
    {
        var outValue = new List<FallbackDeskLedger>();
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
public sealed class CachedFallbackDeskLedgerStore(MemoryFallbackDeskLedgerStore inner, FallbackDeskLedgerCache cache, int hits, int misses) : IFallbackDeskLedgerStore
{
    public MemoryFallbackDeskLedgerStore Inner { get; set; } = inner;
    public FallbackDeskLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackDeskLedger? Find(string code)
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

    public void Save(FallbackDeskLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackDeskLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback desk ledgers. Empty fields match all.</summary>
public sealed record FallbackDeskLedgerQuery(string Category, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackDeskLedger item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackDeskLedger> Apply(List<FallbackDeskLedger> items)
    {
        var matched = Collections.Filter(items, (FallbackDeskLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback desk ledger store.</summary>
public static class FallbackDeskLedgerStore
{
    public static IFallbackDeskLedgerStore NewMemoryFallbackDeskLedgerStore()
    {
        return new MemoryFallbackDeskLedgerStore(new Dictionary<string, FallbackDeskLedger>(), new List<string>());
    }

    public static IFallbackDeskLedgerStore NewCachedFallbackDeskLedgerStore(MemoryFallbackDeskLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackDeskLedgerStore(inner, FallbackDeskLedgerCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FallbackDeskLedger> FindFallbackDeskLedgers(IFallbackDeskLedgerStore store, FallbackDeskLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
