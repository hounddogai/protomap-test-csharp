namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario estimates by code.</summary>
public interface IScenarioEstimateStore
{
    ScenarioEstimate? Find(string code);
    void Save(ScenarioEstimate item);
    bool Remove(string code);
    List<ScenarioEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioEstimateStore(Dictionary<string, ScenarioEstimate> items, List<string> order) : IScenarioEstimateStore
{
    public Dictionary<string, ScenarioEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioEstimate item)
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

    public List<ScenarioEstimate> List()
    {
        var outValue = new List<ScenarioEstimate>();
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
public sealed class CachedScenarioEstimateStore(MemoryScenarioEstimateStore inner, ScenarioEstimateCache cache, int hits, int misses) : IScenarioEstimateStore
{
    public MemoryScenarioEstimateStore Inner { get; set; } = inner;
    public ScenarioEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioEstimate? Find(string code)
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

    public void Save(ScenarioEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario estimates. Empty fields match all.</summary>
public sealed record ScenarioEstimateQuery(string Country, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioEstimate item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioEstimate> Apply(List<ScenarioEstimate> items)
    {
        var matched = Collections.Filter(items, (ScenarioEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario estimate store.</summary>
public static class ScenarioEstimateStore
{
    public static IScenarioEstimateStore NewMemoryScenarioEstimateStore()
    {
        return new MemoryScenarioEstimateStore(new Dictionary<string, ScenarioEstimate>(), new List<string>());
    }

    public static IScenarioEstimateStore NewCachedScenarioEstimateStore(MemoryScenarioEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioEstimateStore(inner, ScenarioEstimateCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ScenarioEstimate> FindScenarioEstimates(IScenarioEstimateStore store, ScenarioEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
