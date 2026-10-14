namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan bundles by code.</summary>
public interface IActionPlanBundleStore
{
    ActionPlanBundle? Find(string code);
    void Save(ActionPlanBundle item);
    bool Remove(string code);
    List<ActionPlanBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanBundleStore(Dictionary<string, ActionPlanBundle> items, List<string> order) : IActionPlanBundleStore
{
    public Dictionary<string, ActionPlanBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanBundle item)
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

    public List<ActionPlanBundle> List()
    {
        var outValue = new List<ActionPlanBundle>();
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
public sealed class CachedActionPlanBundleStore(MemoryActionPlanBundleStore inner, ActionPlanBundleCache cache, int hits, int misses) : IActionPlanBundleStore
{
    public MemoryActionPlanBundleStore Inner { get; set; } = inner;
    public ActionPlanBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanBundle? Find(string code)
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

    public void Save(ActionPlanBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan bundles. Empty fields match all.</summary>
public sealed record ActionPlanBundleQuery(string Region, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanBundle item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanBundle> Apply(List<ActionPlanBundle> items)
    {
        var matched = Collections.Filter(items, (ActionPlanBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan bundle store.</summary>
public static class ActionPlanBundleStore
{
    public static IActionPlanBundleStore NewMemoryActionPlanBundleStore()
    {
        return new MemoryActionPlanBundleStore(new Dictionary<string, ActionPlanBundle>(), new List<string>());
    }

    public static IActionPlanBundleStore NewCachedActionPlanBundleStore(MemoryActionPlanBundleStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanBundleStore(inner, ActionPlanBundleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ActionPlanBundle> FindActionPlanBundles(IActionPlanBundleStore store, ActionPlanBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
