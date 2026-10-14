namespace Acme.Workforce.Compliance.Certification.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive certification adjustments by code.</summary>
public interface IAdaptiveCertificationAdjustmentStore
{
    AdaptiveCertificationAdjustment? Find(string code);
    void Save(AdaptiveCertificationAdjustment item);
    bool Remove(string code);
    List<AdaptiveCertificationAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveCertificationAdjustmentStore(Dictionary<string, AdaptiveCertificationAdjustment> items, List<string> order) : IAdaptiveCertificationAdjustmentStore
{
    public Dictionary<string, AdaptiveCertificationAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveCertificationAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveCertificationAdjustment item)
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

    public List<AdaptiveCertificationAdjustment> List()
    {
        var outValue = new List<AdaptiveCertificationAdjustment>();
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
public sealed class CachedAdaptiveCertificationAdjustmentStore(MemoryAdaptiveCertificationAdjustmentStore inner, AdaptiveCertificationAdjustmentCache cache, int hits, int misses) : IAdaptiveCertificationAdjustmentStore
{
    public MemoryAdaptiveCertificationAdjustmentStore Inner { get; set; } = inner;
    public AdaptiveCertificationAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveCertificationAdjustment? Find(string code)
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

    public void Save(AdaptiveCertificationAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveCertificationAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive certification adjustments. Empty fields match all.</summary>
public sealed record AdaptiveCertificationAdjustmentQuery(string Unit, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveCertificationAdjustment item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<AdaptiveCertificationAdjustment> Apply(List<AdaptiveCertificationAdjustment> items)
    {
        var matched = Collections.Filter(items, (AdaptiveCertificationAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive certification adjustment store.</summary>
public static class AdaptiveCertificationAdjustmentStore
{
    public static IAdaptiveCertificationAdjustmentStore NewMemoryAdaptiveCertificationAdjustmentStore()
    {
        return new MemoryAdaptiveCertificationAdjustmentStore(
            new Dictionary<string, AdaptiveCertificationAdjustment>(),
            new List<string>());
    }

    public static IAdaptiveCertificationAdjustmentStore NewCachedAdaptiveCertificationAdjustmentStore(MemoryAdaptiveCertificationAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveCertificationAdjustmentStore(
            inner,
            AdaptiveCertificationAdjustmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveCertificationAdjustment> FindAdaptiveCertificationAdjustments(IAdaptiveCertificationAdjustmentStore store, AdaptiveCertificationAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
