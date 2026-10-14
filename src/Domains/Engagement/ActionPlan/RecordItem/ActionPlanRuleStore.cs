namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan rules by code.</summary>
public interface IActionPlanRuleStore
{
    ActionPlanRule? Find(string code);
    void Save(ActionPlanRule item);
    bool Remove(string code);
    List<ActionPlanRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanRuleStore(Dictionary<string, ActionPlanRule> items, List<string> order) : IActionPlanRuleStore
{
    public Dictionary<string, ActionPlanRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanRule item)
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

    public List<ActionPlanRule> List()
    {
        var outValue = new List<ActionPlanRule>();
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
public sealed class CachedActionPlanRuleStore(MemoryActionPlanRuleStore inner, ActionPlanRuleCache cache, int hits, int misses) : IActionPlanRuleStore
{
    public MemoryActionPlanRuleStore Inner { get; set; } = inner;
    public ActionPlanRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanRule? Find(string code)
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

    public void Save(ActionPlanRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan rules. Empty fields match all.</summary>
public sealed record ActionPlanRuleQuery(string Unit, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanRule item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ActionPlanRule> Apply(List<ActionPlanRule> items)
    {
        var matched = Collections.Filter(items, (ActionPlanRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan rule store.</summary>
public static class ActionPlanRuleStore
{
    public static IActionPlanRuleStore NewMemoryActionPlanRuleStore()
    {
        return new MemoryActionPlanRuleStore(new Dictionary<string, ActionPlanRule>(), new List<string>());
    }

    public static IActionPlanRuleStore NewCachedActionPlanRuleStore(MemoryActionPlanRuleStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanRuleStore(inner, ActionPlanRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ActionPlanRule> FindActionPlanRules(IActionPlanRuleStore store, ActionPlanRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
