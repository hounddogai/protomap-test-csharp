namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification policies by code.</summary>
public interface ICertificationPolicyStore
{
    CertificationPolicy? Find(string code);
    void Save(CertificationPolicy item);
    bool Remove(string code);
    List<CertificationPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationPolicyStore(Dictionary<string, CertificationPolicy> items, List<string> order) : ICertificationPolicyStore
{
    public Dictionary<string, CertificationPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationPolicy item)
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

    public List<CertificationPolicy> List()
    {
        var outValue = new List<CertificationPolicy>();
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
public sealed class CachedCertificationPolicyStore(MemoryCertificationPolicyStore inner, CertificationPolicyCache cache, int hits, int misses) : ICertificationPolicyStore
{
    public MemoryCertificationPolicyStore Inner { get; set; } = inner;
    public CertificationPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationPolicy? Find(string code)
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

    public void Save(CertificationPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification policies. Empty fields match all.</summary>
public sealed record CertificationPolicyQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationPolicy item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationPolicy> Apply(List<CertificationPolicy> items)
    {
        var matched = Collections.Filter(items, (CertificationPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification policy store.</summary>
public static class CertificationPolicyStore
{
    public static ICertificationPolicyStore NewMemoryCertificationPolicyStore()
    {
        return new MemoryCertificationPolicyStore(new Dictionary<string, CertificationPolicy>(), new List<string>());
    }

    public static ICertificationPolicyStore NewCachedCertificationPolicyStore(MemoryCertificationPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationPolicyStore(inner, CertificationPolicyCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CertificationPolicy> FindCertificationPolicies(ICertificationPolicyStore store, CertificationPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
