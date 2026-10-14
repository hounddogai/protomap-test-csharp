namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan requests by code.</summary>
public interface IActionPlanRequestStore
{
    ActionPlanRequest? Find(string code);
    void Save(ActionPlanRequest item);
    bool Remove(string code);
    List<ActionPlanRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanRequestStore(Dictionary<string, ActionPlanRequest> items, List<string> order) : IActionPlanRequestStore
{
    public Dictionary<string, ActionPlanRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanRequest item)
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

    public List<ActionPlanRequest> List()
    {
        var outValue = new List<ActionPlanRequest>();
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
public sealed class CachedActionPlanRequestStore(MemoryActionPlanRequestStore inner, ActionPlanRequestCache cache, int hits, int misses) : IActionPlanRequestStore
{
    public MemoryActionPlanRequestStore Inner { get; set; } = inner;
    public ActionPlanRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanRequest? Find(string code)
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

    public void Save(ActionPlanRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan requests. Empty fields match all.</summary>
public sealed record ActionPlanRequestQuery(string Department, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanRequest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanRequest> Apply(List<ActionPlanRequest> items)
    {
        var matched = Collections.Filter(items, (ActionPlanRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan request store.</summary>
public static class ActionPlanRequestStore
{
    public static IActionPlanRequestStore NewMemoryActionPlanRequestStore()
    {
        return new MemoryActionPlanRequestStore(new Dictionary<string, ActionPlanRequest>(), new List<string>());
    }

    public static IActionPlanRequestStore NewCachedActionPlanRequestStore(MemoryActionPlanRequestStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanRequestStore(inner, ActionPlanRequestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ActionPlanRequest> FindActionPlanRequests(IActionPlanRequestStore store, ActionPlanRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
