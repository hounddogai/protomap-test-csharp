namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification claims by code.</summary>
public interface ICertificationClaimStore
{
    CertificationClaim? Find(string code);
    void Save(CertificationClaim item);
    bool Remove(string code);
    List<CertificationClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationClaimStore(Dictionary<string, CertificationClaim> items, List<string> order) : ICertificationClaimStore
{
    public Dictionary<string, CertificationClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationClaim item)
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

    public List<CertificationClaim> List()
    {
        var outValue = new List<CertificationClaim>();
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
public sealed class CachedCertificationClaimStore(MemoryCertificationClaimStore inner, CertificationClaimCache cache, int hits, int misses) : ICertificationClaimStore
{
    public MemoryCertificationClaimStore Inner { get; set; } = inner;
    public CertificationClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationClaim? Find(string code)
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

    public void Save(CertificationClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification claims. Empty fields match all.</summary>
public sealed record CertificationClaimQuery(string Channel, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationClaim item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationClaim> Apply(List<CertificationClaim> items)
    {
        var matched = Collections.Filter(items, (CertificationClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification claim store.</summary>
public static class CertificationClaimStore
{
    public static ICertificationClaimStore NewMemoryCertificationClaimStore()
    {
        return new MemoryCertificationClaimStore(new Dictionary<string, CertificationClaim>(), new List<string>());
    }

    public static ICertificationClaimStore NewCachedCertificationClaimStore(MemoryCertificationClaimStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationClaimStore(inner, CertificationClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CertificationClaim> FindCertificationClaims(ICertificationClaimStore store, CertificationClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
