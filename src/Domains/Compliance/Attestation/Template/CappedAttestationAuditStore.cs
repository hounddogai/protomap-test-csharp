namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped attestation audits by code.</summary>
public interface ICappedAttestationAuditStore
{
    CappedAttestationAudit? Find(string code);
    void Save(CappedAttestationAudit item);
    bool Remove(string code);
    List<CappedAttestationAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAttestationAuditStore(Dictionary<string, CappedAttestationAudit> items, List<string> order) : ICappedAttestationAuditStore
{
    public Dictionary<string, CappedAttestationAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAttestationAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAttestationAudit item)
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

    public List<CappedAttestationAudit> List()
    {
        var outValue = new List<CappedAttestationAudit>();
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
public sealed class CachedCappedAttestationAuditStore(MemoryCappedAttestationAuditStore inner, CappedAttestationAuditCache cache, int hits, int misses) : ICappedAttestationAuditStore
{
    public MemoryCappedAttestationAuditStore Inner { get; set; } = inner;
    public CappedAttestationAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAttestationAudit? Find(string code)
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

    public void Save(CappedAttestationAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAttestationAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped attestation audits. Empty fields match all.</summary>
public sealed record CappedAttestationAuditQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAttestationAudit item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAttestationAudit> Apply(List<CappedAttestationAudit> items)
    {
        var matched = Collections.Filter(items, (CappedAttestationAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped attestation audit store.</summary>
public static class CappedAttestationAuditStore
{
    public static ICappedAttestationAuditStore NewMemoryCappedAttestationAuditStore()
    {
        return new MemoryCappedAttestationAuditStore(
            new Dictionary<string, CappedAttestationAudit>(),
            new List<string>());
    }

    public static ICappedAttestationAuditStore NewCachedCappedAttestationAuditStore(MemoryCappedAttestationAuditStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAttestationAuditStore(
            inner,
            CappedAttestationAuditCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedAttestationAudit> FindCappedAttestationAudits(ICappedAttestationAuditStore store, CappedAttestationAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
