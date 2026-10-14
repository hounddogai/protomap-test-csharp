namespace Acme.Workforce.Assets.Assignment.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment rules by code.</summary>
public interface IAssignmentRuleStore
{
    AssignmentRule? Find(string code);
    void Save(AssignmentRule item);
    bool Remove(string code);
    List<AssignmentRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentRuleStore(Dictionary<string, AssignmentRule> items, List<string> order) : IAssignmentRuleStore
{
    public Dictionary<string, AssignmentRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentRule item)
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

    public List<AssignmentRule> List()
    {
        var outValue = new List<AssignmentRule>();
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
public sealed class CachedAssignmentRuleStore(MemoryAssignmentRuleStore inner, AssignmentRuleCache cache, int hits, int misses) : IAssignmentRuleStore
{
    public MemoryAssignmentRuleStore Inner { get; set; } = inner;
    public AssignmentRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentRule? Find(string code)
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

    public void Save(AssignmentRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment rules. Empty fields match all.</summary>
public sealed record AssignmentRuleQuery(string Country, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentRule item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentRule> Apply(List<AssignmentRule> items)
    {
        var matched = Collections.Filter(items, (AssignmentRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment rule store.</summary>
public static class AssignmentRuleStore
{
    public static IAssignmentRuleStore NewMemoryAssignmentRuleStore()
    {
        return new MemoryAssignmentRuleStore(new Dictionary<string, AssignmentRule>(), new List<string>());
    }

    public static IAssignmentRuleStore NewCachedAssignmentRuleStore(MemoryAssignmentRuleStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentRuleStore(inner, AssignmentRuleCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssignmentRule> FindAssignmentRules(IAssignmentRuleStore store, AssignmentRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
