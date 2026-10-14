namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default adjustment estimates by code.</summary>
public interface IDefaultAdjustmentEstimateStore
{
    DefaultAdjustmentEstimate? Find(string code);
    void Save(DefaultAdjustmentEstimate item);
    bool Remove(string code);
    List<DefaultAdjustmentEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAdjustmentEstimateStore(Dictionary<string, DefaultAdjustmentEstimate> items, List<string> order) : IDefaultAdjustmentEstimateStore
{
    public Dictionary<string, DefaultAdjustmentEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAdjustmentEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAdjustmentEstimate item)
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

    public List<DefaultAdjustmentEstimate> List()
    {
        var outValue = new List<DefaultAdjustmentEstimate>();
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
public sealed class CachedDefaultAdjustmentEstimateStore(MemoryDefaultAdjustmentEstimateStore inner, DefaultAdjustmentEstimateCache cache, int hits, int misses) : IDefaultAdjustmentEstimateStore
{
    public MemoryDefaultAdjustmentEstimateStore Inner { get; set; } = inner;
    public DefaultAdjustmentEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAdjustmentEstimate? Find(string code)
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

    public void Save(DefaultAdjustmentEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAdjustmentEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default adjustment estimates. Empty fields match all.</summary>
public sealed record DefaultAdjustmentEstimateQuery(string Region, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAdjustmentEstimate item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<DefaultAdjustmentEstimate> Apply(List<DefaultAdjustmentEstimate> items)
    {
        var matched = Collections.Filter(items, (DefaultAdjustmentEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default adjustment estimate store.</summary>
public static class DefaultAdjustmentEstimateStore
{
    public static IDefaultAdjustmentEstimateStore NewMemoryDefaultAdjustmentEstimateStore()
    {
        return new MemoryDefaultAdjustmentEstimateStore(
            new Dictionary<string, DefaultAdjustmentEstimate>(),
            new List<string>());
    }

    public static IDefaultAdjustmentEstimateStore NewCachedDefaultAdjustmentEstimateStore(MemoryDefaultAdjustmentEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAdjustmentEstimateStore(
            inner,
            DefaultAdjustmentEstimateCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultAdjustmentEstimate> FindDefaultAdjustmentEstimates(IDefaultAdjustmentEstimateStore store, DefaultAdjustmentEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
