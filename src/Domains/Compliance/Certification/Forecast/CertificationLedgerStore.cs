namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification ledgers by code.</summary>
public interface ICertificationLedgerStore
{
    CertificationLedger? Find(string code);
    void Save(CertificationLedger item);
    bool Remove(string code);
    List<CertificationLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationLedgerStore(Dictionary<string, CertificationLedger> items, List<string> order) : ICertificationLedgerStore
{
    public Dictionary<string, CertificationLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationLedger item)
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

    public List<CertificationLedger> List()
    {
        var outValue = new List<CertificationLedger>();
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
public sealed class CachedCertificationLedgerStore(MemoryCertificationLedgerStore inner, CertificationLedgerCache cache, int hits, int misses) : ICertificationLedgerStore
{
    public MemoryCertificationLedgerStore Inner { get; set; } = inner;
    public CertificationLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationLedger? Find(string code)
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

    public void Save(CertificationLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification ledgers. Empty fields match all.</summary>
public sealed record CertificationLedgerQuery(string Category, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationLedger item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationLedger> Apply(List<CertificationLedger> items)
    {
        var matched = Collections.Filter(items, (CertificationLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification ledger store.</summary>
public static class CertificationLedgerStore
{
    public static ICertificationLedgerStore NewMemoryCertificationLedgerStore()
    {
        return new MemoryCertificationLedgerStore(new Dictionary<string, CertificationLedger>(), new List<string>());
    }

    public static ICertificationLedgerStore NewCachedCertificationLedgerStore(MemoryCertificationLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationLedgerStore(inner, CertificationLedgerCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CertificationLedger> FindCertificationLedgers(ICertificationLedgerStore store, CertificationLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
