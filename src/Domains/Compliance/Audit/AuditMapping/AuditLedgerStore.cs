namespace Acme.Workforce.Compliance.Audit.AuditMapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps audit ledgers by code.</summary>
public interface IAuditLedgerStore
{
    AuditLedger? Find(string code);
    void Save(AuditLedger item);
    bool Remove(string code);
    List<AuditLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAuditLedgerStore(Dictionary<string, AuditLedger> items, List<string> order) : IAuditLedgerStore
{
    public Dictionary<string, AuditLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AuditLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AuditLedger item)
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

    public List<AuditLedger> List()
    {
        var outValue = new List<AuditLedger>();
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
public sealed class CachedAuditLedgerStore(MemoryAuditLedgerStore inner, AuditLedgerCache cache, int hits, int misses) : IAuditLedgerStore
{
    public MemoryAuditLedgerStore Inner { get; set; } = inner;
    public AuditLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AuditLedger? Find(string code)
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

    public void Save(AuditLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AuditLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects audit ledgers. Empty fields match all.</summary>
public sealed record AuditLedgerQuery(string Locale, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AuditLedger item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AuditLedger> Apply(List<AuditLedger> items)
    {
        var matched = Collections.Filter(items, (AuditLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of audit ledger store.</summary>
public static class AuditLedgerStore
{
    public static IAuditLedgerStore NewMemoryAuditLedgerStore()
    {
        return new MemoryAuditLedgerStore(new Dictionary<string, AuditLedger>(), new List<string>());
    }

    public static IAuditLedgerStore NewCachedAuditLedgerStore(MemoryAuditLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedAuditLedgerStore(inner, AuditLedgerCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AuditLedger> FindAuditLedgers(IAuditLedgerStore store, AuditLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
