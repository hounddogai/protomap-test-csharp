namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario targets by code.</summary>
public interface IScenarioTargetStore
{
    ScenarioTarget? Find(string code);
    void Save(ScenarioTarget item);
    bool Remove(string code);
    List<ScenarioTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioTargetStore(Dictionary<string, ScenarioTarget> items, List<string> order) : IScenarioTargetStore
{
    public Dictionary<string, ScenarioTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioTarget item)
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

    public List<ScenarioTarget> List()
    {
        var outValue = new List<ScenarioTarget>();
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
public sealed class CachedScenarioTargetStore(MemoryScenarioTargetStore inner, ScenarioTargetCache cache, int hits, int misses) : IScenarioTargetStore
{
    public MemoryScenarioTargetStore Inner { get; set; } = inner;
    public ScenarioTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioTarget? Find(string code)
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

    public void Save(ScenarioTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario targets. Empty fields match all.</summary>
public sealed record ScenarioTargetQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioTarget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioTarget> Apply(List<ScenarioTarget> items)
    {
        var matched = Collections.Filter(items, (ScenarioTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario target store.</summary>
public static class ScenarioTargetStore
{
    public static IScenarioTargetStore NewMemoryScenarioTargetStore()
    {
        return new MemoryScenarioTargetStore(new Dictionary<string, ScenarioTarget>(), new List<string>());
    }

    public static IScenarioTargetStore NewCachedScenarioTargetStore(MemoryScenarioTargetStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioTargetStore(inner, ScenarioTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ScenarioTarget> FindScenarioTargets(IScenarioTargetStore store, ScenarioTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
