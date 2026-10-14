namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation variances by code.</summary>
public interface IAttestationVarianceStore
{
    AttestationVariance? Find(string code);
    void Save(AttestationVariance item);
    bool Remove(string code);
    List<AttestationVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationVarianceStore(Dictionary<string, AttestationVariance> items, List<string> order) : IAttestationVarianceStore
{
    public Dictionary<string, AttestationVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationVariance item)
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

    public List<AttestationVariance> List()
    {
        var outValue = new List<AttestationVariance>();
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
public sealed class CachedAttestationVarianceStore(MemoryAttestationVarianceStore inner, AttestationVarianceCache cache, int hits, int misses) : IAttestationVarianceStore
{
    public MemoryAttestationVarianceStore Inner { get; set; } = inner;
    public AttestationVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationVariance? Find(string code)
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

    public void Save(AttestationVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation variances. Empty fields match all.</summary>
public sealed record AttestationVarianceQuery(string Region, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationVariance item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationVariance> Apply(List<AttestationVariance> items)
    {
        var matched = Collections.Filter(items, (AttestationVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation variance store.</summary>
public static class AttestationVarianceStore
{
    public static IAttestationVarianceStore NewMemoryAttestationVarianceStore()
    {
        return new MemoryAttestationVarianceStore(new Dictionary<string, AttestationVariance>(), new List<string>());
    }

    public static IAttestationVarianceStore NewCachedAttestationVarianceStore(MemoryAttestationVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationVarianceStore(inner, AttestationVarianceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AttestationVariance> FindAttestationVariances(IAttestationVarianceStore store, AttestationVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
