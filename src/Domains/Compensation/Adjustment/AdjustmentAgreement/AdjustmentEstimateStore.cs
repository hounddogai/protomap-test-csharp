namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment estimates by code.</summary>
public interface IAdjustmentEstimateStore
{
    AdjustmentEstimate? Find(string code);
    void Save(AdjustmentEstimate item);
    bool Remove(string code);
    List<AdjustmentEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentEstimateStore(Dictionary<string, AdjustmentEstimate> items, List<string> order) : IAdjustmentEstimateStore
{
    public Dictionary<string, AdjustmentEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentEstimate item)
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

    public List<AdjustmentEstimate> List()
    {
        var outValue = new List<AdjustmentEstimate>();
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
public sealed class CachedAdjustmentEstimateStore(MemoryAdjustmentEstimateStore inner, AdjustmentEstimateCache cache, int hits, int misses) : IAdjustmentEstimateStore
{
    public MemoryAdjustmentEstimateStore Inner { get; set; } = inner;
    public AdjustmentEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentEstimate? Find(string code)
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

    public void Save(AdjustmentEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment estimates. Empty fields match all.</summary>
public sealed record AdjustmentEstimateQuery(string Department, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentEstimate item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<AdjustmentEstimate> Apply(List<AdjustmentEstimate> items)
    {
        var matched = Collections.Filter(items, (AdjustmentEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment estimate store.</summary>
public static class AdjustmentEstimateStore
{
    public static IAdjustmentEstimateStore NewMemoryAdjustmentEstimateStore()
    {
        return new MemoryAdjustmentEstimateStore(new Dictionary<string, AdjustmentEstimate>(), new List<string>());
    }

    public static IAdjustmentEstimateStore NewCachedAdjustmentEstimateStore(MemoryAdjustmentEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentEstimateStore(inner, AdjustmentEstimateCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AdjustmentEstimate> FindAdjustmentEstimates(IAdjustmentEstimateStore store, AdjustmentEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
