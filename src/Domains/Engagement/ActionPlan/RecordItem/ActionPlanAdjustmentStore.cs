namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan adjustments by code.</summary>
public interface IActionPlanAdjustmentStore
{
    ActionPlanAdjustment? Find(string code);
    void Save(ActionPlanAdjustment item);
    bool Remove(string code);
    List<ActionPlanAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanAdjustmentStore(Dictionary<string, ActionPlanAdjustment> items, List<string> order) : IActionPlanAdjustmentStore
{
    public Dictionary<string, ActionPlanAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanAdjustment item)
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

    public List<ActionPlanAdjustment> List()
    {
        var outValue = new List<ActionPlanAdjustment>();
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
public sealed class CachedActionPlanAdjustmentStore(MemoryActionPlanAdjustmentStore inner, ActionPlanAdjustmentCache cache, int hits, int misses) : IActionPlanAdjustmentStore
{
    public MemoryActionPlanAdjustmentStore Inner { get; set; } = inner;
    public ActionPlanAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanAdjustment? Find(string code)
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

    public void Save(ActionPlanAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan adjustments. Empty fields match all.</summary>
public sealed record ActionPlanAdjustmentQuery(string Country, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanAdjustment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanAdjustment> Apply(List<ActionPlanAdjustment> items)
    {
        var matched = Collections.Filter(items, (ActionPlanAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan adjustment store.</summary>
public static class ActionPlanAdjustmentStore
{
    public static IActionPlanAdjustmentStore NewMemoryActionPlanAdjustmentStore()
    {
        return new MemoryActionPlanAdjustmentStore(
            new Dictionary<string, ActionPlanAdjustment>(),
            new List<string>());
    }

    public static IActionPlanAdjustmentStore NewCachedActionPlanAdjustmentStore(MemoryActionPlanAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanAdjustmentStore(inner, ActionPlanAdjustmentCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ActionPlanAdjustment> FindActionPlanAdjustments(IActionPlanAdjustmentStore store, ActionPlanAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
