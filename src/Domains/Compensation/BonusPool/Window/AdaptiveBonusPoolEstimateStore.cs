namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive bonus pool estimates by code.</summary>
public interface IAdaptiveBonusPoolEstimateStore
{
    AdaptiveBonusPoolEstimate? Find(string code);
    void Save(AdaptiveBonusPoolEstimate item);
    bool Remove(string code);
    List<AdaptiveBonusPoolEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBonusPoolEstimateStore(Dictionary<string, AdaptiveBonusPoolEstimate> items, List<string> order) : IAdaptiveBonusPoolEstimateStore
{
    public Dictionary<string, AdaptiveBonusPoolEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBonusPoolEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBonusPoolEstimate item)
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

    public List<AdaptiveBonusPoolEstimate> List()
    {
        var outValue = new List<AdaptiveBonusPoolEstimate>();
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
public sealed class CachedAdaptiveBonusPoolEstimateStore(MemoryAdaptiveBonusPoolEstimateStore inner, AdaptiveBonusPoolEstimateCache cache, int hits, int misses) : IAdaptiveBonusPoolEstimateStore
{
    public MemoryAdaptiveBonusPoolEstimateStore Inner { get; set; } = inner;
    public AdaptiveBonusPoolEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBonusPoolEstimate? Find(string code)
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

    public void Save(AdaptiveBonusPoolEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBonusPoolEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive bonus pool estimates. Empty fields match all.</summary>
public sealed record AdaptiveBonusPoolEstimateQuery(string Name, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBonusPoolEstimate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<AdaptiveBonusPoolEstimate> Apply(List<AdaptiveBonusPoolEstimate> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBonusPoolEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive bonus pool estimate store.</summary>
public static class AdaptiveBonusPoolEstimateStore
{
    public static IAdaptiveBonusPoolEstimateStore NewMemoryAdaptiveBonusPoolEstimateStore()
    {
        return new MemoryAdaptiveBonusPoolEstimateStore(
            new Dictionary<string, AdaptiveBonusPoolEstimate>(),
            new List<string>());
    }

    public static IAdaptiveBonusPoolEstimateStore NewCachedAdaptiveBonusPoolEstimateStore(MemoryAdaptiveBonusPoolEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBonusPoolEstimateStore(
            inner,
            AdaptiveBonusPoolEstimateCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBonusPoolEstimate> FindAdaptiveBonusPoolEstimates(IAdaptiveBonusPoolEstimateStore store, AdaptiveBonusPoolEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
