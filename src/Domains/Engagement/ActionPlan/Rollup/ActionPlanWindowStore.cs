namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan windows by code.</summary>
public interface IActionPlanWindowStore
{
    ActionPlanWindow? Find(string code);
    void Save(ActionPlanWindow item);
    bool Remove(string code);
    List<ActionPlanWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanWindowStore(Dictionary<string, ActionPlanWindow> items, List<string> order) : IActionPlanWindowStore
{
    public Dictionary<string, ActionPlanWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanWindow item)
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

    public List<ActionPlanWindow> List()
    {
        var outValue = new List<ActionPlanWindow>();
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
public sealed class CachedActionPlanWindowStore(MemoryActionPlanWindowStore inner, ActionPlanWindowCache cache, int hits, int misses) : IActionPlanWindowStore
{
    public MemoryActionPlanWindowStore Inner { get; set; } = inner;
    public ActionPlanWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanWindow? Find(string code)
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

    public void Save(ActionPlanWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan windows. Empty fields match all.</summary>
public sealed record ActionPlanWindowQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanWindow item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<ActionPlanWindow> Apply(List<ActionPlanWindow> items)
    {
        var matched = Collections.Filter(items, (ActionPlanWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan window store.</summary>
public static class ActionPlanWindowStore
{
    public static IActionPlanWindowStore NewMemoryActionPlanWindowStore()
    {
        return new MemoryActionPlanWindowStore(new Dictionary<string, ActionPlanWindow>(), new List<string>());
    }

    public static IActionPlanWindowStore NewCachedActionPlanWindowStore(MemoryActionPlanWindowStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanWindowStore(inner, ActionPlanWindowCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ActionPlanWindow> FindActionPlanWindows(IActionPlanWindowStore store, ActionPlanWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
