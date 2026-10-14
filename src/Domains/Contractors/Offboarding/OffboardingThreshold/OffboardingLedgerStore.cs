namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding ledgers by code.</summary>
public interface IOffboardingLedgerStore
{
    OffboardingLedger? Find(string code);
    void Save(OffboardingLedger item);
    bool Remove(string code);
    List<OffboardingLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingLedgerStore(Dictionary<string, OffboardingLedger> items, List<string> order) : IOffboardingLedgerStore
{
    public Dictionary<string, OffboardingLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingLedger item)
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

    public List<OffboardingLedger> List()
    {
        var outValue = new List<OffboardingLedger>();
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
public sealed class CachedOffboardingLedgerStore(MemoryOffboardingLedgerStore inner, OffboardingLedgerCache cache, int hits, int misses) : IOffboardingLedgerStore
{
    public MemoryOffboardingLedgerStore Inner { get; set; } = inner;
    public OffboardingLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingLedger? Find(string code)
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

    public void Save(OffboardingLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding ledgers. Empty fields match all.</summary>
public sealed record OffboardingLedgerQuery(string Category, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingLedger item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingLedger> Apply(List<OffboardingLedger> items)
    {
        var matched = Collections.Filter(items, (OffboardingLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding ledger store.</summary>
public static class OffboardingLedgerStore
{
    public static IOffboardingLedgerStore NewMemoryOffboardingLedgerStore()
    {
        return new MemoryOffboardingLedgerStore(new Dictionary<string, OffboardingLedger>(), new List<string>());
    }

    public static IOffboardingLedgerStore NewCachedOffboardingLedgerStore(MemoryOffboardingLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingLedgerStore(inner, OffboardingLedgerCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<OffboardingLedger> FindOffboardingLedgers(IOffboardingLedgerStore store, OffboardingLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
