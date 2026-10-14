namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation quotas by code.</summary>
public interface IAttestationQuotaStore
{
    AttestationQuota? Find(string code);
    void Save(AttestationQuota item);
    bool Remove(string code);
    List<AttestationQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationQuotaStore(Dictionary<string, AttestationQuota> items, List<string> order) : IAttestationQuotaStore
{
    public Dictionary<string, AttestationQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationQuota item)
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

    public List<AttestationQuota> List()
    {
        var outValue = new List<AttestationQuota>();
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
public sealed class CachedAttestationQuotaStore(MemoryAttestationQuotaStore inner, AttestationQuotaCache cache, int hits, int misses) : IAttestationQuotaStore
{
    public MemoryAttestationQuotaStore Inner { get; set; } = inner;
    public AttestationQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationQuota? Find(string code)
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

    public void Save(AttestationQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation quotas. Empty fields match all.</summary>
public sealed record AttestationQuotaQuery(string Source, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationQuota item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationQuota> Apply(List<AttestationQuota> items)
    {
        var matched = Collections.Filter(items, (AttestationQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation quota store.</summary>
public static class AttestationQuotaStore
{
    public static IAttestationQuotaStore NewMemoryAttestationQuotaStore()
    {
        return new MemoryAttestationQuotaStore(new Dictionary<string, AttestationQuota>(), new List<string>());
    }

    public static IAttestationQuotaStore NewCachedAttestationQuotaStore(MemoryAttestationQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationQuotaStore(inner, AttestationQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AttestationQuota> FindAttestationQuotas(IAttestationQuotaStore store, AttestationQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
