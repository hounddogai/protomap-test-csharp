namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled action plan adjustments by code.</summary>
public interface IScheduledActionPlanAdjustmentStore
{
    ScheduledActionPlanAdjustment? Find(string code);
    void Save(ScheduledActionPlanAdjustment item);
    bool Remove(string code);
    List<ScheduledActionPlanAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledActionPlanAdjustmentStore(Dictionary<string, ScheduledActionPlanAdjustment> items, List<string> order) : IScheduledActionPlanAdjustmentStore
{
    public Dictionary<string, ScheduledActionPlanAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledActionPlanAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledActionPlanAdjustment item)
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

    public List<ScheduledActionPlanAdjustment> List()
    {
        var outValue = new List<ScheduledActionPlanAdjustment>();
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
public sealed class CachedScheduledActionPlanAdjustmentStore(MemoryScheduledActionPlanAdjustmentStore inner, ScheduledActionPlanAdjustmentCache cache, int hits, int misses) : IScheduledActionPlanAdjustmentStore
{
    public MemoryScheduledActionPlanAdjustmentStore Inner { get; set; } = inner;
    public ScheduledActionPlanAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledActionPlanAdjustment? Find(string code)
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

    public void Save(ScheduledActionPlanAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledActionPlanAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled action plan adjustments. Empty fields match all.</summary>
public sealed record ScheduledActionPlanAdjustmentQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledActionPlanAdjustment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledActionPlanAdjustment> Apply(List<ScheduledActionPlanAdjustment> items)
    {
        var matched = Collections.Filter(items, (ScheduledActionPlanAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled action plan adjustment store.</summary>
public static class ScheduledActionPlanAdjustmentStore
{
    public static IScheduledActionPlanAdjustmentStore NewMemoryScheduledActionPlanAdjustmentStore()
    {
        return new MemoryScheduledActionPlanAdjustmentStore(
            new Dictionary<string, ScheduledActionPlanAdjustment>(),
            new List<string>());
    }

    public static IScheduledActionPlanAdjustmentStore NewCachedScheduledActionPlanAdjustmentStore(MemoryScheduledActionPlanAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledActionPlanAdjustmentStore(
            inner,
            ScheduledActionPlanAdjustmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledActionPlanAdjustment> FindScheduledActionPlanAdjustments(IScheduledActionPlanAdjustmentStore store, ScheduledActionPlanAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
