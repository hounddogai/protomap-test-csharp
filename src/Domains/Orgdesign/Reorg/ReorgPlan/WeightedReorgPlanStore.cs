namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted reorg plans by code.</summary>
public interface IWeightedReorgPlanStore
{
    WeightedReorgPlan? Find(string code);
    void Save(WeightedReorgPlan item);
    bool Remove(string code);
    List<WeightedReorgPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedReorgPlanStore(Dictionary<string, WeightedReorgPlan> items, List<string> order) : IWeightedReorgPlanStore
{
    public Dictionary<string, WeightedReorgPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedReorgPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedReorgPlan item)
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

    public List<WeightedReorgPlan> List()
    {
        var outValue = new List<WeightedReorgPlan>();
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
public sealed class CachedWeightedReorgPlanStore(MemoryWeightedReorgPlanStore inner, WeightedReorgPlanCache cache, int hits, int misses) : IWeightedReorgPlanStore
{
    public MemoryWeightedReorgPlanStore Inner { get; set; } = inner;
    public WeightedReorgPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedReorgPlan? Find(string code)
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

    public void Save(WeightedReorgPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedReorgPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted reorg plans. Empty fields match all.</summary>
public sealed record WeightedReorgPlanQuery(string Segment, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedReorgPlan item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedReorgPlan> Apply(List<WeightedReorgPlan> items)
    {
        var matched = Collections.Filter(items, (WeightedReorgPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted reorg plan store.</summary>
public static class WeightedReorgPlanStore
{
    public static IWeightedReorgPlanStore NewMemoryWeightedReorgPlanStore()
    {
        return new MemoryWeightedReorgPlanStore(new Dictionary<string, WeightedReorgPlan>(), new List<string>());
    }

    public static IWeightedReorgPlanStore NewCachedWeightedReorgPlanStore(MemoryWeightedReorgPlanStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedReorgPlanStore(inner, WeightedReorgPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<WeightedReorgPlan> FindWeightedReorgPlans(IWeightedReorgPlanStore store, WeightedReorgPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
