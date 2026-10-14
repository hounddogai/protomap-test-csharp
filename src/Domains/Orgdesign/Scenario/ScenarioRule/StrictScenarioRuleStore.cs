namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict scenario rules by code.</summary>
public interface IStrictScenarioRuleStore
{
    StrictScenarioRule? Find(string code);
    void Save(StrictScenarioRule item);
    bool Remove(string code);
    List<StrictScenarioRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictScenarioRuleStore(Dictionary<string, StrictScenarioRule> items, List<string> order) : IStrictScenarioRuleStore
{
    public Dictionary<string, StrictScenarioRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictScenarioRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictScenarioRule item)
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

    public List<StrictScenarioRule> List()
    {
        var outValue = new List<StrictScenarioRule>();
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
public sealed class CachedStrictScenarioRuleStore(MemoryStrictScenarioRuleStore inner, StrictScenarioRuleCache cache, int hits, int misses) : IStrictScenarioRuleStore
{
    public MemoryStrictScenarioRuleStore Inner { get; set; } = inner;
    public StrictScenarioRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictScenarioRule? Find(string code)
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

    public void Save(StrictScenarioRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictScenarioRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict scenario rules. Empty fields match all.</summary>
public sealed record StrictScenarioRuleQuery(string Category, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(StrictScenarioRule item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictScenarioRule> Apply(List<StrictScenarioRule> items)
    {
        var matched = Collections.Filter(items, (StrictScenarioRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict scenario rule store.</summary>
public static class StrictScenarioRuleStore
{
    public static IStrictScenarioRuleStore NewMemoryStrictScenarioRuleStore()
    {
        return new MemoryStrictScenarioRuleStore(new Dictionary<string, StrictScenarioRule>(), new List<string>());
    }

    public static IStrictScenarioRuleStore NewCachedStrictScenarioRuleStore(MemoryStrictScenarioRuleStore inner, int ttl, IClock clock)
    {
        return new CachedStrictScenarioRuleStore(inner, StrictScenarioRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictScenarioRule> FindStrictScenarioRules(IStrictScenarioRuleStore store, StrictScenarioRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
