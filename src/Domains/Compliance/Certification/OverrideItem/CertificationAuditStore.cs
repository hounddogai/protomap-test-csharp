namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification audits by code.</summary>
public interface ICertificationAuditStore
{
    CertificationAudit? Find(string code);
    void Save(CertificationAudit item);
    bool Remove(string code);
    List<CertificationAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationAuditStore(Dictionary<string, CertificationAudit> items, List<string> order) : ICertificationAuditStore
{
    public Dictionary<string, CertificationAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationAudit item)
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

    public List<CertificationAudit> List()
    {
        var outValue = new List<CertificationAudit>();
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
public sealed class CachedCertificationAuditStore(MemoryCertificationAuditStore inner, CertificationAuditCache cache, int hits, int misses) : ICertificationAuditStore
{
    public MemoryCertificationAuditStore Inner { get; set; } = inner;
    public CertificationAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationAudit? Find(string code)
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

    public void Save(CertificationAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification audits. Empty fields match all.</summary>
public sealed record CertificationAuditQuery(string Label, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationAudit> Apply(List<CertificationAudit> items)
    {
        var matched = Collections.Filter(items, (CertificationAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification audit store.</summary>
public static class CertificationAuditStore
{
    public static ICertificationAuditStore NewMemoryCertificationAuditStore()
    {
        return new MemoryCertificationAuditStore(new Dictionary<string, CertificationAudit>(), new List<string>());
    }

    public static ICertificationAuditStore NewCachedCertificationAuditStore(MemoryCertificationAuditStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationAuditStore(inner, CertificationAuditCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CertificationAudit> FindCertificationAudits(ICertificationAuditStore store, CertificationAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
