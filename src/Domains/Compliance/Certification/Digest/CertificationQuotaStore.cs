namespace Acme.Workforce.Compliance.Certification.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification quotas by code.</summary>
public interface ICertificationQuotaStore
{
    CertificationQuota? Find(string code);
    void Save(CertificationQuota item);
    bool Remove(string code);
    List<CertificationQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationQuotaStore(Dictionary<string, CertificationQuota> items, List<string> order) : ICertificationQuotaStore
{
    public Dictionary<string, CertificationQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationQuota item)
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

    public List<CertificationQuota> List()
    {
        var outValue = new List<CertificationQuota>();
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
public sealed class CachedCertificationQuotaStore(MemoryCertificationQuotaStore inner, CertificationQuotaCache cache, int hits, int misses) : ICertificationQuotaStore
{
    public MemoryCertificationQuotaStore Inner { get; set; } = inner;
    public CertificationQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationQuota? Find(string code)
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

    public void Save(CertificationQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification quotas. Empty fields match all.</summary>
public sealed record CertificationQuotaQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationQuota item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationQuota> Apply(List<CertificationQuota> items)
    {
        var matched = Collections.Filter(items, (CertificationQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification quota store.</summary>
public static class CertificationQuotaStore
{
    public static ICertificationQuotaStore NewMemoryCertificationQuotaStore()
    {
        return new MemoryCertificationQuotaStore(new Dictionary<string, CertificationQuota>(), new List<string>());
    }

    public static ICertificationQuotaStore NewCachedCertificationQuotaStore(MemoryCertificationQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationQuotaStore(inner, CertificationQuotaCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CertificationQuota> FindCertificationQuotas(ICertificationQuotaStore store, CertificationQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
