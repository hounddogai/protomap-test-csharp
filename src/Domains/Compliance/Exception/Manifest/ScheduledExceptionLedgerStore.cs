namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled exception ledgers by code.</summary>
public interface IScheduledExceptionLedgerStore
{
    ScheduledExceptionLedger? Find(string code);
    void Save(ScheduledExceptionLedger item);
    bool Remove(string code);
    List<ScheduledExceptionLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledExceptionLedgerStore(Dictionary<string, ScheduledExceptionLedger> items, List<string> order) : IScheduledExceptionLedgerStore
{
    public Dictionary<string, ScheduledExceptionLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledExceptionLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledExceptionLedger item)
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

    public List<ScheduledExceptionLedger> List()
    {
        var outValue = new List<ScheduledExceptionLedger>();
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
public sealed class CachedScheduledExceptionLedgerStore(MemoryScheduledExceptionLedgerStore inner, ScheduledExceptionLedgerCache cache, int hits, int misses) : IScheduledExceptionLedgerStore
{
    public MemoryScheduledExceptionLedgerStore Inner { get; set; } = inner;
    public ScheduledExceptionLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledExceptionLedger? Find(string code)
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

    public void Save(ScheduledExceptionLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledExceptionLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled exception ledgers. Empty fields match all.</summary>
public sealed record ScheduledExceptionLedgerQuery(string Locale, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledExceptionLedger item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledExceptionLedger> Apply(List<ScheduledExceptionLedger> items)
    {
        var matched = Collections.Filter(items, (ScheduledExceptionLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled exception ledger store.</summary>
public static class ScheduledExceptionLedgerStore
{
    public static IScheduledExceptionLedgerStore NewMemoryScheduledExceptionLedgerStore()
    {
        return new MemoryScheduledExceptionLedgerStore(
            new Dictionary<string, ScheduledExceptionLedger>(),
            new List<string>());
    }

    public static IScheduledExceptionLedgerStore NewCachedScheduledExceptionLedgerStore(MemoryScheduledExceptionLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledExceptionLedgerStore(
            inner,
            ScheduledExceptionLedgerCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledExceptionLedger> FindScheduledExceptionLedgers(IScheduledExceptionLedgerStore store, ScheduledExceptionLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
