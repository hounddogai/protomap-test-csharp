namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan cycles by code.</summary>
public interface IActionPlanCycleStore
{
    ActionPlanCycle? Find(string code);
    void Save(ActionPlanCycle item);
    bool Remove(string code);
    List<ActionPlanCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanCycleStore(Dictionary<string, ActionPlanCycle> items, List<string> order) : IActionPlanCycleStore
{
    public Dictionary<string, ActionPlanCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanCycle item)
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

    public List<ActionPlanCycle> List()
    {
        var outValue = new List<ActionPlanCycle>();
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
public sealed class CachedActionPlanCycleStore(MemoryActionPlanCycleStore inner, ActionPlanCycleCache cache, int hits, int misses) : IActionPlanCycleStore
{
    public MemoryActionPlanCycleStore Inner { get; set; } = inner;
    public ActionPlanCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanCycle? Find(string code)
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

    public void Save(ActionPlanCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan cycles. Empty fields match all.</summary>
public sealed record ActionPlanCycleQuery(string Team, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanCycle item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanCycle> Apply(List<ActionPlanCycle> items)
    {
        var matched = Collections.Filter(items, (ActionPlanCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan cycle store.</summary>
public static class ActionPlanCycleStore
{
    public static IActionPlanCycleStore NewMemoryActionPlanCycleStore()
    {
        return new MemoryActionPlanCycleStore(new Dictionary<string, ActionPlanCycle>(), new List<string>());
    }

    public static IActionPlanCycleStore NewCachedActionPlanCycleStore(MemoryActionPlanCycleStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanCycleStore(inner, ActionPlanCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ActionPlanCycle> FindActionPlanCycles(IActionPlanCycleStore store, ActionPlanCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
