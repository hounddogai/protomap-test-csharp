namespace Acme.Workforce.Compliance.Certification.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification batches by code.</summary>
public interface ICertificationBatchStore
{
    CertificationBatch? Find(string code);
    void Save(CertificationBatch item);
    bool Remove(string code);
    List<CertificationBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationBatchStore(Dictionary<string, CertificationBatch> items, List<string> order) : ICertificationBatchStore
{
    public Dictionary<string, CertificationBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationBatch item)
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

    public List<CertificationBatch> List()
    {
        var outValue = new List<CertificationBatch>();
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
public sealed class CachedCertificationBatchStore(MemoryCertificationBatchStore inner, CertificationBatchCache cache, int hits, int misses) : ICertificationBatchStore
{
    public MemoryCertificationBatchStore Inner { get; set; } = inner;
    public CertificationBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationBatch? Find(string code)
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

    public void Save(CertificationBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification batches. Empty fields match all.</summary>
public sealed record CertificationBatchQuery(string Department, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationBatch item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationBatch> Apply(List<CertificationBatch> items)
    {
        var matched = Collections.Filter(items, (CertificationBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification batch store.</summary>
public static class CertificationBatchStore
{
    public static ICertificationBatchStore NewMemoryCertificationBatchStore()
    {
        return new MemoryCertificationBatchStore(new Dictionary<string, CertificationBatch>(), new List<string>());
    }

    public static ICertificationBatchStore NewCachedCertificationBatchStore(MemoryCertificationBatchStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationBatchStore(inner, CertificationBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CertificationBatch> FindCertificationBatches(ICertificationBatchStore store, CertificationBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
