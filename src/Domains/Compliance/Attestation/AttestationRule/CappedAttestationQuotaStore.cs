namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped attestation quotas by code.</summary>
public interface ICappedAttestationQuotaStore
{
    CappedAttestationQuota? Find(string code);
    void Save(CappedAttestationQuota item);
    bool Remove(string code);
    List<CappedAttestationQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAttestationQuotaStore(Dictionary<string, CappedAttestationQuota> items, List<string> order) : ICappedAttestationQuotaStore
{
    public Dictionary<string, CappedAttestationQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAttestationQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAttestationQuota item)
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

    public List<CappedAttestationQuota> List()
    {
        var outValue = new List<CappedAttestationQuota>();
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
public sealed class CachedCappedAttestationQuotaStore(MemoryCappedAttestationQuotaStore inner, CappedAttestationQuotaCache cache, int hits, int misses) : ICappedAttestationQuotaStore
{
    public MemoryCappedAttestationQuotaStore Inner { get; set; } = inner;
    public CappedAttestationQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAttestationQuota? Find(string code)
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

    public void Save(CappedAttestationQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAttestationQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped attestation quotas. Empty fields match all.</summary>
public sealed record CappedAttestationQuotaQuery(string Segment, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAttestationQuota item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAttestationQuota> Apply(List<CappedAttestationQuota> items)
    {
        var matched = Collections.Filter(items, (CappedAttestationQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped attestation quota store.</summary>
public static class CappedAttestationQuotaStore
{
    public static ICappedAttestationQuotaStore NewMemoryCappedAttestationQuotaStore()
    {
        return new MemoryCappedAttestationQuotaStore(
            new Dictionary<string, CappedAttestationQuota>(),
            new List<string>());
    }

    public static ICappedAttestationQuotaStore NewCachedCappedAttestationQuotaStore(MemoryCappedAttestationQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAttestationQuotaStore(
            inner,
            CappedAttestationQuotaCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedAttestationQuota> FindCappedAttestationQuotas(ICappedAttestationQuotaStore store, CappedAttestationQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
