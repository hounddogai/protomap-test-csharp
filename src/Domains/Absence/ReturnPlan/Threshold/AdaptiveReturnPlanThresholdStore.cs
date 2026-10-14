namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive return plan thresholds by code.</summary>
public interface IAdaptiveReturnPlanThresholdStore
{
    AdaptiveReturnPlanThreshold? Find(string code);
    void Save(AdaptiveReturnPlanThreshold item);
    bool Remove(string code);
    List<AdaptiveReturnPlanThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveReturnPlanThresholdStore(Dictionary<string, AdaptiveReturnPlanThreshold> items, List<string> order) : IAdaptiveReturnPlanThresholdStore
{
    public Dictionary<string, AdaptiveReturnPlanThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveReturnPlanThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveReturnPlanThreshold item)
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

    public List<AdaptiveReturnPlanThreshold> List()
    {
        var outValue = new List<AdaptiveReturnPlanThreshold>();
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
public sealed class CachedAdaptiveReturnPlanThresholdStore(MemoryAdaptiveReturnPlanThresholdStore inner, AdaptiveReturnPlanThresholdCache cache, int hits, int misses) : IAdaptiveReturnPlanThresholdStore
{
    public MemoryAdaptiveReturnPlanThresholdStore Inner { get; set; } = inner;
    public AdaptiveReturnPlanThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveReturnPlanThreshold? Find(string code)
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

    public void Save(AdaptiveReturnPlanThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveReturnPlanThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive return plan thresholds. Empty fields match all.</summary>
public sealed record AdaptiveReturnPlanThresholdQuery(string Department, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveReturnPlanThreshold item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveReturnPlanThreshold> Apply(List<AdaptiveReturnPlanThreshold> items)
    {
        var matched = Collections.Filter(items, (AdaptiveReturnPlanThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive return plan threshold store.</summary>
public static class AdaptiveReturnPlanThresholdStore
{
    public static IAdaptiveReturnPlanThresholdStore NewMemoryAdaptiveReturnPlanThresholdStore()
    {
        return new MemoryAdaptiveReturnPlanThresholdStore(
            new Dictionary<string, AdaptiveReturnPlanThreshold>(),
            new List<string>());
    }

    public static IAdaptiveReturnPlanThresholdStore NewCachedAdaptiveReturnPlanThresholdStore(MemoryAdaptiveReturnPlanThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveReturnPlanThresholdStore(
            inner,
            AdaptiveReturnPlanThresholdCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveReturnPlanThreshold> FindAdaptiveReturnPlanThresholds(IAdaptiveReturnPlanThresholdStore store, AdaptiveReturnPlanThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
