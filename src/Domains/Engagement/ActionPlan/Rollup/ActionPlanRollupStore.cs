namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan rollups by code.</summary>
public interface IActionPlanRollupStore
{
    ActionPlanRollup? Find(string code);
    void Save(ActionPlanRollup item);
    bool Remove(string code);
    List<ActionPlanRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanRollupStore(Dictionary<string, ActionPlanRollup> items, List<string> order) : IActionPlanRollupStore
{
    public Dictionary<string, ActionPlanRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanRollup item)
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

    public List<ActionPlanRollup> List()
    {
        var outValue = new List<ActionPlanRollup>();
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
public sealed class CachedActionPlanRollupStore(MemoryActionPlanRollupStore inner, ActionPlanRollupCache cache, int hits, int misses) : IActionPlanRollupStore
{
    public MemoryActionPlanRollupStore Inner { get; set; } = inner;
    public ActionPlanRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanRollup? Find(string code)
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

    public void Save(ActionPlanRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan rollups. Empty fields match all.</summary>
public sealed record ActionPlanRollupQuery(string Category, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanRollup item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanRollup> Apply(List<ActionPlanRollup> items)
    {
        var matched = Collections.Filter(items, (ActionPlanRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan rollup store.</summary>
public static class ActionPlanRollupStore
{
    public static IActionPlanRollupStore NewMemoryActionPlanRollupStore()
    {
        return new MemoryActionPlanRollupStore(new Dictionary<string, ActionPlanRollup>(), new List<string>());
    }

    public static IActionPlanRollupStore NewCachedActionPlanRollupStore(MemoryActionPlanRollupStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanRollupStore(inner, ActionPlanRollupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ActionPlanRollup> FindActionPlanRollups(IActionPlanRollupStore store, ActionPlanRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
