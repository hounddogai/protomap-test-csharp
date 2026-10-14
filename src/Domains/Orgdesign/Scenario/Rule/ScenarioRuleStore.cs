namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario rules by code.</summary>
public interface IScenarioRuleStore
{
    ScenarioRule? Find(string code);
    void Save(ScenarioRule item);
    bool Remove(string code);
    List<ScenarioRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioRuleStore(Dictionary<string, ScenarioRule> items, List<string> order) : IScenarioRuleStore
{
    public Dictionary<string, ScenarioRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioRule item)
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

    public List<ScenarioRule> List()
    {
        var outValue = new List<ScenarioRule>();
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
public sealed class CachedScenarioRuleStore(MemoryScenarioRuleStore inner, ScenarioRuleCache cache, int hits, int misses) : IScenarioRuleStore
{
    public MemoryScenarioRuleStore Inner { get; set; } = inner;
    public ScenarioRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioRule? Find(string code)
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

    public void Save(ScenarioRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario rules. Empty fields match all.</summary>
public sealed record ScenarioRuleQuery(string Category, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioRule item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioRule> Apply(List<ScenarioRule> items)
    {
        var matched = Collections.Filter(items, (ScenarioRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario rule store.</summary>
public static class ScenarioRuleStore
{
    public static IScenarioRuleStore NewMemoryScenarioRuleStore()
    {
        return new MemoryScenarioRuleStore(new Dictionary<string, ScenarioRule>(), new List<string>());
    }

    public static IScenarioRuleStore NewCachedScenarioRuleStore(MemoryScenarioRuleStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioRuleStore(inner, ScenarioRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ScenarioRule> FindScenarioRules(IScenarioRuleStore store, ScenarioRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
