namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual attestation estimates by code.</summary>
public interface IManualAttestationEstimateStore
{
    ManualAttestationEstimate? Find(string code);
    void Save(ManualAttestationEstimate item);
    bool Remove(string code);
    List<ManualAttestationEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualAttestationEstimateStore(Dictionary<string, ManualAttestationEstimate> items, List<string> order) : IManualAttestationEstimateStore
{
    public Dictionary<string, ManualAttestationEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualAttestationEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualAttestationEstimate item)
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

    public List<ManualAttestationEstimate> List()
    {
        var outValue = new List<ManualAttestationEstimate>();
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
public sealed class CachedManualAttestationEstimateStore(MemoryManualAttestationEstimateStore inner, ManualAttestationEstimateCache cache, int hits, int misses) : IManualAttestationEstimateStore
{
    public MemoryManualAttestationEstimateStore Inner { get; set; } = inner;
    public ManualAttestationEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualAttestationEstimate? Find(string code)
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

    public void Save(ManualAttestationEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualAttestationEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual attestation estimates. Empty fields match all.</summary>
public sealed record ManualAttestationEstimateQuery(string Country, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ManualAttestationEstimate item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualAttestationEstimate> Apply(List<ManualAttestationEstimate> items)
    {
        var matched = Collections.Filter(items, (ManualAttestationEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual attestation estimate store.</summary>
public static class ManualAttestationEstimateStore
{
    public static IManualAttestationEstimateStore NewMemoryManualAttestationEstimateStore()
    {
        return new MemoryManualAttestationEstimateStore(
            new Dictionary<string, ManualAttestationEstimate>(),
            new List<string>());
    }

    public static IManualAttestationEstimateStore NewCachedManualAttestationEstimateStore(MemoryManualAttestationEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedManualAttestationEstimateStore(
            inner,
            ManualAttestationEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualAttestationEstimate> FindManualAttestationEstimates(IManualAttestationEstimateStore store, ManualAttestationEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
