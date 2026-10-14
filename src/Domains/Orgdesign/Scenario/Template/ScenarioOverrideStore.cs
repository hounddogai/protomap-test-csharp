namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario overrides by code.</summary>
public interface IScenarioOverrideStore
{
    ScenarioOverride? Find(string code);
    void Save(ScenarioOverride item);
    bool Remove(string code);
    List<ScenarioOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioOverrideStore(Dictionary<string, ScenarioOverride> items, List<string> order) : IScenarioOverrideStore
{
    public Dictionary<string, ScenarioOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioOverride item)
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

    public List<ScenarioOverride> List()
    {
        var outValue = new List<ScenarioOverride>();
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
public sealed class CachedScenarioOverrideStore(MemoryScenarioOverrideStore inner, ScenarioOverrideCache cache, int hits, int misses) : IScenarioOverrideStore
{
    public MemoryScenarioOverrideStore Inner { get; set; } = inner;
    public ScenarioOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioOverride? Find(string code)
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

    public void Save(ScenarioOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario overrides. Empty fields match all.</summary>
public sealed record ScenarioOverrideQuery(string Unit, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioOverride item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<ScenarioOverride> Apply(List<ScenarioOverride> items)
    {
        var matched = Collections.Filter(items, (ScenarioOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario override store.</summary>
public static class ScenarioOverrideStore
{
    public static IScenarioOverrideStore NewMemoryScenarioOverrideStore()
    {
        return new MemoryScenarioOverrideStore(new Dictionary<string, ScenarioOverride>(), new List<string>());
    }

    public static IScenarioOverrideStore NewCachedScenarioOverrideStore(MemoryScenarioOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioOverrideStore(inner, ScenarioOverrideCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ScenarioOverride> FindScenarioOverrides(IScenarioOverrideStore store, ScenarioOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
