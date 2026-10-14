namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted return plan overrides by code.</summary>
public interface IWeightedReturnPlanOverrideStore
{
    WeightedReturnPlanOverride? Find(string code);
    void Save(WeightedReturnPlanOverride item);
    bool Remove(string code);
    List<WeightedReturnPlanOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedReturnPlanOverrideStore(Dictionary<string, WeightedReturnPlanOverride> items, List<string> order) : IWeightedReturnPlanOverrideStore
{
    public Dictionary<string, WeightedReturnPlanOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedReturnPlanOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedReturnPlanOverride item)
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

    public List<WeightedReturnPlanOverride> List()
    {
        var outValue = new List<WeightedReturnPlanOverride>();
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
public sealed class CachedWeightedReturnPlanOverrideStore(MemoryWeightedReturnPlanOverrideStore inner, WeightedReturnPlanOverrideCache cache, int hits, int misses) : IWeightedReturnPlanOverrideStore
{
    public MemoryWeightedReturnPlanOverrideStore Inner { get; set; } = inner;
    public WeightedReturnPlanOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedReturnPlanOverride? Find(string code)
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

    public void Save(WeightedReturnPlanOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedReturnPlanOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted return plan overrides. Empty fields match all.</summary>
public sealed record WeightedReturnPlanOverrideQuery(string Category, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedReturnPlanOverride item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedReturnPlanOverride> Apply(List<WeightedReturnPlanOverride> items)
    {
        var matched = Collections.Filter(items, (WeightedReturnPlanOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted return plan override store.</summary>
public static class WeightedReturnPlanOverrideStore
{
    public static IWeightedReturnPlanOverrideStore NewMemoryWeightedReturnPlanOverrideStore()
    {
        return new MemoryWeightedReturnPlanOverrideStore(
            new Dictionary<string, WeightedReturnPlanOverride>(),
            new List<string>());
    }

    public static IWeightedReturnPlanOverrideStore NewCachedWeightedReturnPlanOverrideStore(MemoryWeightedReturnPlanOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedReturnPlanOverrideStore(
            inner,
            WeightedReturnPlanOverrideCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedReturnPlanOverride> FindWeightedReturnPlanOverrides(IWeightedReturnPlanOverrideStore store, WeightedReturnPlanOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
