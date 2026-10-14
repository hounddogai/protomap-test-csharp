namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario policies by code.</summary>
public interface IScenarioPolicyStore
{
    ScenarioPolicy? Find(string code);
    void Save(ScenarioPolicy item);
    bool Remove(string code);
    List<ScenarioPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioPolicyStore(Dictionary<string, ScenarioPolicy> items, List<string> order) : IScenarioPolicyStore
{
    public Dictionary<string, ScenarioPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioPolicy item)
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

    public List<ScenarioPolicy> List()
    {
        var outValue = new List<ScenarioPolicy>();
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
public sealed class CachedScenarioPolicyStore(MemoryScenarioPolicyStore inner, ScenarioPolicyCache cache, int hits, int misses) : IScenarioPolicyStore
{
    public MemoryScenarioPolicyStore Inner { get; set; } = inner;
    public ScenarioPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioPolicy? Find(string code)
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

    public void Save(ScenarioPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario policies. Empty fields match all.</summary>
public sealed record ScenarioPolicyQuery(string Team, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioPolicy item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioPolicy> Apply(List<ScenarioPolicy> items)
    {
        var matched = Collections.Filter(items, (ScenarioPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario policy store.</summary>
public static class ScenarioPolicyStore
{
    public static IScenarioPolicyStore NewMemoryScenarioPolicyStore()
    {
        return new MemoryScenarioPolicyStore(new Dictionary<string, ScenarioPolicy>(), new List<string>());
    }

    public static IScenarioPolicyStore NewCachedScenarioPolicyStore(MemoryScenarioPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioPolicyStore(inner, ScenarioPolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ScenarioPolicy> FindScenarioPolicies(IScenarioPolicyStore store, ScenarioPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
