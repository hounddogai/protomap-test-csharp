namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment plans by code.</summary>
public interface IAdjustmentPlanStore
{
    AdjustmentPlan? Find(string code);
    void Save(AdjustmentPlan item);
    bool Remove(string code);
    List<AdjustmentPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentPlanStore(Dictionary<string, AdjustmentPlan> items, List<string> order) : IAdjustmentPlanStore
{
    public Dictionary<string, AdjustmentPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentPlan item)
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

    public List<AdjustmentPlan> List()
    {
        var outValue = new List<AdjustmentPlan>();
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
public sealed class CachedAdjustmentPlanStore(MemoryAdjustmentPlanStore inner, AdjustmentPlanCache cache, int hits, int misses) : IAdjustmentPlanStore
{
    public MemoryAdjustmentPlanStore Inner { get; set; } = inner;
    public AdjustmentPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentPlan? Find(string code)
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

    public void Save(AdjustmentPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment plans. Empty fields match all.</summary>
public sealed record AdjustmentPlanQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentPlan item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<AdjustmentPlan> Apply(List<AdjustmentPlan> items)
    {
        var matched = Collections.Filter(items, (AdjustmentPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment plan store.</summary>
public static class AdjustmentPlanStore
{
    public static IAdjustmentPlanStore NewMemoryAdjustmentPlanStore()
    {
        return new MemoryAdjustmentPlanStore(new Dictionary<string, AdjustmentPlan>(), new List<string>());
    }

    public static IAdjustmentPlanStore NewCachedAdjustmentPlanStore(MemoryAdjustmentPlanStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentPlanStore(inner, AdjustmentPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AdjustmentPlan> FindAdjustmentPlans(IAdjustmentPlanStore store, AdjustmentPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
