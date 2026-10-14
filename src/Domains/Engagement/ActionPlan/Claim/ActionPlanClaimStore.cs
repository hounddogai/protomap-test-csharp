namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan claims by code.</summary>
public interface IActionPlanClaimStore
{
    ActionPlanClaim? Find(string code);
    void Save(ActionPlanClaim item);
    bool Remove(string code);
    List<ActionPlanClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanClaimStore(Dictionary<string, ActionPlanClaim> items, List<string> order) : IActionPlanClaimStore
{
    public Dictionary<string, ActionPlanClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanClaim item)
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

    public List<ActionPlanClaim> List()
    {
        var outValue = new List<ActionPlanClaim>();
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
public sealed class CachedActionPlanClaimStore(MemoryActionPlanClaimStore inner, ActionPlanClaimCache cache, int hits, int misses) : IActionPlanClaimStore
{
    public MemoryActionPlanClaimStore Inner { get; set; } = inner;
    public ActionPlanClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanClaim? Find(string code)
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

    public void Save(ActionPlanClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan claims. Empty fields match all.</summary>
public sealed record ActionPlanClaimQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanClaim item)
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

    public List<ActionPlanClaim> Apply(List<ActionPlanClaim> items)
    {
        var matched = Collections.Filter(items, (ActionPlanClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan claim store.</summary>
public static class ActionPlanClaimStore
{
    public static IActionPlanClaimStore NewMemoryActionPlanClaimStore()
    {
        return new MemoryActionPlanClaimStore(new Dictionary<string, ActionPlanClaim>(), new List<string>());
    }

    public static IActionPlanClaimStore NewCachedActionPlanClaimStore(MemoryActionPlanClaimStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanClaimStore(inner, ActionPlanClaimCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ActionPlanClaim> FindActionPlanClaims(IActionPlanClaimStore store, ActionPlanClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
