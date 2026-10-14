namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled equity grant ledgers by code.</summary>
public interface IScheduledEquityGrantLedgerStore
{
    ScheduledEquityGrantLedger? Find(string code);
    void Save(ScheduledEquityGrantLedger item);
    bool Remove(string code);
    List<ScheduledEquityGrantLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledEquityGrantLedgerStore(Dictionary<string, ScheduledEquityGrantLedger> items, List<string> order) : IScheduledEquityGrantLedgerStore
{
    public Dictionary<string, ScheduledEquityGrantLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledEquityGrantLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledEquityGrantLedger item)
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

    public List<ScheduledEquityGrantLedger> List()
    {
        var outValue = new List<ScheduledEquityGrantLedger>();
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
public sealed class CachedScheduledEquityGrantLedgerStore(MemoryScheduledEquityGrantLedgerStore inner, ScheduledEquityGrantLedgerCache cache, int hits, int misses) : IScheduledEquityGrantLedgerStore
{
    public MemoryScheduledEquityGrantLedgerStore Inner { get; set; } = inner;
    public ScheduledEquityGrantLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledEquityGrantLedger? Find(string code)
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

    public void Save(ScheduledEquityGrantLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledEquityGrantLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled equity grant ledgers. Empty fields match all.</summary>
public sealed record ScheduledEquityGrantLedgerQuery(string Locale, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledEquityGrantLedger item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<ScheduledEquityGrantLedger> Apply(List<ScheduledEquityGrantLedger> items)
    {
        var matched = Collections.Filter(items, (ScheduledEquityGrantLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled equity grant ledger store.</summary>
public static class ScheduledEquityGrantLedgerStore
{
    public static IScheduledEquityGrantLedgerStore NewMemoryScheduledEquityGrantLedgerStore()
    {
        return new MemoryScheduledEquityGrantLedgerStore(
            new Dictionary<string, ScheduledEquityGrantLedger>(),
            new List<string>());
    }

    public static IScheduledEquityGrantLedgerStore NewCachedScheduledEquityGrantLedgerStore(MemoryScheduledEquityGrantLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledEquityGrantLedgerStore(
            inner,
            ScheduledEquityGrantLedgerCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledEquityGrantLedger> FindScheduledEquityGrantLedgers(IScheduledEquityGrantLedgerStore store, ScheduledEquityGrantLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
