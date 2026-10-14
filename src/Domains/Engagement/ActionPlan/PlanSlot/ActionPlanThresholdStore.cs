namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan thresholds by code.</summary>
public interface IActionPlanThresholdStore
{
    ActionPlanThreshold? Find(string code);
    void Save(ActionPlanThreshold item);
    bool Remove(string code);
    List<ActionPlanThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanThresholdStore(Dictionary<string, ActionPlanThreshold> items, List<string> order) : IActionPlanThresholdStore
{
    public Dictionary<string, ActionPlanThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanThreshold item)
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

    public List<ActionPlanThreshold> List()
    {
        var outValue = new List<ActionPlanThreshold>();
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
public sealed class CachedActionPlanThresholdStore(MemoryActionPlanThresholdStore inner, ActionPlanThresholdCache cache, int hits, int misses) : IActionPlanThresholdStore
{
    public MemoryActionPlanThresholdStore Inner { get; set; } = inner;
    public ActionPlanThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanThreshold? Find(string code)
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

    public void Save(ActionPlanThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan thresholds. Empty fields match all.</summary>
public sealed record ActionPlanThresholdQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanThreshold item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanThreshold> Apply(List<ActionPlanThreshold> items)
    {
        var matched = Collections.Filter(items, (ActionPlanThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan threshold store.</summary>
public static class ActionPlanThresholdStore
{
    public static IActionPlanThresholdStore NewMemoryActionPlanThresholdStore()
    {
        return new MemoryActionPlanThresholdStore(new Dictionary<string, ActionPlanThreshold>(), new List<string>());
    }

    public static IActionPlanThresholdStore NewCachedActionPlanThresholdStore(MemoryActionPlanThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanThresholdStore(inner, ActionPlanThresholdCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ActionPlanThreshold> FindActionPlanThresholds(IActionPlanThresholdStore store, ActionPlanThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
