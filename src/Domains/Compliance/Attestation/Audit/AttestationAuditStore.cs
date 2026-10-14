namespace Acme.Workforce.Compliance.Attestation.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation audits by code.</summary>
public interface IAttestationAuditStore
{
    AttestationAudit? Find(string code);
    void Save(AttestationAudit item);
    bool Remove(string code);
    List<AttestationAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationAuditStore(Dictionary<string, AttestationAudit> items, List<string> order) : IAttestationAuditStore
{
    public Dictionary<string, AttestationAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationAudit item)
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

    public List<AttestationAudit> List()
    {
        var outValue = new List<AttestationAudit>();
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
public sealed class CachedAttestationAuditStore(MemoryAttestationAuditStore inner, AttestationAuditCache cache, int hits, int misses) : IAttestationAuditStore
{
    public MemoryAttestationAuditStore Inner { get; set; } = inner;
    public AttestationAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationAudit? Find(string code)
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

    public void Save(AttestationAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation audits. Empty fields match all.</summary>
public sealed record AttestationAuditQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationAudit item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationAudit> Apply(List<AttestationAudit> items)
    {
        var matched = Collections.Filter(items, (AttestationAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation audit store.</summary>
public static class AttestationAuditStore
{
    public static IAttestationAuditStore NewMemoryAttestationAuditStore()
    {
        return new MemoryAttestationAuditStore(new Dictionary<string, AttestationAudit>(), new List<string>());
    }

    public static IAttestationAuditStore NewCachedAttestationAuditStore(MemoryAttestationAuditStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationAuditStore(inner, AttestationAuditCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AttestationAudit> FindAttestationAudits(IAttestationAuditStore store, AttestationAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
