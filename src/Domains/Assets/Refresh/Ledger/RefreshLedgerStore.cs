namespace Acme.Workforce.Assets.Refresh.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh ledgers by code.</summary>
public interface IRefreshLedgerStore
{
    RefreshLedger? Find(string code);
    void Save(RefreshLedger item);
    bool Remove(string code);
    List<RefreshLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshLedgerStore(Dictionary<string, RefreshLedger> items, List<string> order) : IRefreshLedgerStore
{
    public Dictionary<string, RefreshLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshLedger item)
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

    public List<RefreshLedger> List()
    {
        var outValue = new List<RefreshLedger>();
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
public sealed class CachedRefreshLedgerStore(MemoryRefreshLedgerStore inner, RefreshLedgerCache cache, int hits, int misses) : IRefreshLedgerStore
{
    public MemoryRefreshLedgerStore Inner { get; set; } = inner;
    public RefreshLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshLedger? Find(string code)
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

    public void Save(RefreshLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh ledgers. Empty fields match all.</summary>
public sealed record RefreshLedgerQuery(string Category, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshLedger item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshLedger> Apply(List<RefreshLedger> items)
    {
        var matched = Collections.Filter(items, (RefreshLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh ledger store.</summary>
public static class RefreshLedgerStore
{
    public static IRefreshLedgerStore NewMemoryRefreshLedgerStore()
    {
        return new MemoryRefreshLedgerStore(new Dictionary<string, RefreshLedger>(), new List<string>());
    }

    public static IRefreshLedgerStore NewCachedRefreshLedgerStore(MemoryRefreshLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshLedgerStore(inner, RefreshLedgerCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RefreshLedger> FindRefreshLedgers(IRefreshLedgerStore store, RefreshLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
