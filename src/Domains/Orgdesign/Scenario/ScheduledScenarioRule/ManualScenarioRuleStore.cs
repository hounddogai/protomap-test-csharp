namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual scenario rules by code.</summary>
public interface IManualScenarioRuleStore
{
    ManualScenarioRule? Find(string code);
    void Save(ManualScenarioRule item);
    bool Remove(string code);
    List<ManualScenarioRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualScenarioRuleStore(Dictionary<string, ManualScenarioRule> items, List<string> order) : IManualScenarioRuleStore
{
    public Dictionary<string, ManualScenarioRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualScenarioRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualScenarioRule item)
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

    public List<ManualScenarioRule> List()
    {
        var outValue = new List<ManualScenarioRule>();
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
public sealed class CachedManualScenarioRuleStore(MemoryManualScenarioRuleStore inner, ManualScenarioRuleCache cache, int hits, int misses) : IManualScenarioRuleStore
{
    public MemoryManualScenarioRuleStore Inner { get; set; } = inner;
    public ManualScenarioRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualScenarioRule? Find(string code)
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

    public void Save(ManualScenarioRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualScenarioRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual scenario rules. Empty fields match all.</summary>
public sealed record ManualScenarioRuleQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(ManualScenarioRule item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<ManualScenarioRule> Apply(List<ManualScenarioRule> items)
    {
        var matched = Collections.Filter(items, (ManualScenarioRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual scenario rule store.</summary>
public static class ManualScenarioRuleStore
{
    public static IManualScenarioRuleStore NewMemoryManualScenarioRuleStore()
    {
        return new MemoryManualScenarioRuleStore(new Dictionary<string, ManualScenarioRule>(), new List<string>());
    }

    public static IManualScenarioRuleStore NewCachedManualScenarioRuleStore(MemoryManualScenarioRuleStore inner, int ttl, IClock clock)
    {
        return new CachedManualScenarioRuleStore(inner, ManualScenarioRuleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ManualScenarioRule> FindManualScenarioRules(IManualScenarioRuleStore store, ManualScenarioRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
