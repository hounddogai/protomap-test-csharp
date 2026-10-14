namespace Acme.Workforce.Compliance.Certification.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted certification batches by code.</summary>
public interface IWeightedCertificationBatchStore
{
    WeightedCertificationBatch? Find(string code);
    void Save(WeightedCertificationBatch item);
    bool Remove(string code);
    List<WeightedCertificationBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedCertificationBatchStore(Dictionary<string, WeightedCertificationBatch> items, List<string> order) : IWeightedCertificationBatchStore
{
    public Dictionary<string, WeightedCertificationBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedCertificationBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedCertificationBatch item)
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

    public List<WeightedCertificationBatch> List()
    {
        var outValue = new List<WeightedCertificationBatch>();
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
public sealed class CachedWeightedCertificationBatchStore(MemoryWeightedCertificationBatchStore inner, WeightedCertificationBatchCache cache, int hits, int misses) : IWeightedCertificationBatchStore
{
    public MemoryWeightedCertificationBatchStore Inner { get; set; } = inner;
    public WeightedCertificationBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedCertificationBatch? Find(string code)
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

    public void Save(WeightedCertificationBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedCertificationBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted certification batches. Empty fields match all.</summary>
public sealed record WeightedCertificationBatchQuery(string Country, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedCertificationBatch item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedCertificationBatch> Apply(List<WeightedCertificationBatch> items)
    {
        var matched = Collections.Filter(items, (WeightedCertificationBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted certification batch store.</summary>
public static class WeightedCertificationBatchStore
{
    public static IWeightedCertificationBatchStore NewMemoryWeightedCertificationBatchStore()
    {
        return new MemoryWeightedCertificationBatchStore(
            new Dictionary<string, WeightedCertificationBatch>(),
            new List<string>());
    }

    public static IWeightedCertificationBatchStore NewCachedWeightedCertificationBatchStore(MemoryWeightedCertificationBatchStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedCertificationBatchStore(
            inner,
            WeightedCertificationBatchCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedCertificationBatch> FindWeightedCertificationBatches(IWeightedCertificationBatchStore store, WeightedCertificationBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
