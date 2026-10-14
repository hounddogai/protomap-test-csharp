namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification adjustments by code.</summary>
public interface ICertificationAdjustmentStore
{
    CertificationAdjustment? Find(string code);
    void Save(CertificationAdjustment item);
    bool Remove(string code);
    List<CertificationAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationAdjustmentStore(Dictionary<string, CertificationAdjustment> items, List<string> order) : ICertificationAdjustmentStore
{
    public Dictionary<string, CertificationAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationAdjustment item)
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

    public List<CertificationAdjustment> List()
    {
        var outValue = new List<CertificationAdjustment>();
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
public sealed class CachedCertificationAdjustmentStore(MemoryCertificationAdjustmentStore inner, CertificationAdjustmentCache cache, int hits, int misses) : ICertificationAdjustmentStore
{
    public MemoryCertificationAdjustmentStore Inner { get; set; } = inner;
    public CertificationAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationAdjustment? Find(string code)
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

    public void Save(CertificationAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification adjustments. Empty fields match all.</summary>
public sealed record CertificationAdjustmentQuery(string Label, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationAdjustment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationAdjustment> Apply(List<CertificationAdjustment> items)
    {
        var matched = Collections.Filter(items, (CertificationAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification adjustment store.</summary>
public static class CertificationAdjustmentStore
{
    public static ICertificationAdjustmentStore NewMemoryCertificationAdjustmentStore()
    {
        return new MemoryCertificationAdjustmentStore(
            new Dictionary<string, CertificationAdjustment>(),
            new List<string>());
    }

    public static ICertificationAdjustmentStore NewCachedCertificationAdjustmentStore(MemoryCertificationAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationAdjustmentStore(
            inner,
            CertificationAdjustmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CertificationAdjustment> FindCertificationAdjustments(ICertificationAdjustmentStore store, CertificationAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
