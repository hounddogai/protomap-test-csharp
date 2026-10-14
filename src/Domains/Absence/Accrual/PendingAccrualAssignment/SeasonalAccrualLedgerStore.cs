namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal accrual ledgers by code.</summary>
public interface ISeasonalAccrualLedgerStore
{
    SeasonalAccrualLedger? Find(string code);
    void Save(SeasonalAccrualLedger item);
    bool Remove(string code);
    List<SeasonalAccrualLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAccrualLedgerStore(Dictionary<string, SeasonalAccrualLedger> items, List<string> order) : ISeasonalAccrualLedgerStore
{
    public Dictionary<string, SeasonalAccrualLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAccrualLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAccrualLedger item)
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

    public List<SeasonalAccrualLedger> List()
    {
        var outValue = new List<SeasonalAccrualLedger>();
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
public sealed class CachedSeasonalAccrualLedgerStore(MemorySeasonalAccrualLedgerStore inner, SeasonalAccrualLedgerCache cache, int hits, int misses) : ISeasonalAccrualLedgerStore
{
    public MemorySeasonalAccrualLedgerStore Inner { get; set; } = inner;
    public SeasonalAccrualLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAccrualLedger? Find(string code)
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

    public void Save(SeasonalAccrualLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAccrualLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal accrual ledgers. Empty fields match all.</summary>
public sealed record SeasonalAccrualLedgerQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAccrualLedger item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<SeasonalAccrualLedger> Apply(List<SeasonalAccrualLedger> items)
    {
        var matched = Collections.Filter(items, (SeasonalAccrualLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal accrual ledger store.</summary>
public static class SeasonalAccrualLedgerStore
{
    public static ISeasonalAccrualLedgerStore NewMemorySeasonalAccrualLedgerStore()
    {
        return new MemorySeasonalAccrualLedgerStore(
            new Dictionary<string, SeasonalAccrualLedger>(),
            new List<string>());
    }

    public static ISeasonalAccrualLedgerStore NewCachedSeasonalAccrualLedgerStore(MemorySeasonalAccrualLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAccrualLedgerStore(inner, SeasonalAccrualLedgerCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SeasonalAccrualLedger> FindSeasonalAccrualLedgers(ISeasonalAccrualLedgerStore store, SeasonalAccrualLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
