namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario thresholds by code.</summary>
public interface IScenarioThresholdStore
{
    ScenarioThreshold? Find(string code);
    void Save(ScenarioThreshold item);
    bool Remove(string code);
    List<ScenarioThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioThresholdStore(Dictionary<string, ScenarioThreshold> items, List<string> order) : IScenarioThresholdStore
{
    public Dictionary<string, ScenarioThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioThreshold item)
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

    public List<ScenarioThreshold> List()
    {
        var outValue = new List<ScenarioThreshold>();
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
public sealed class CachedScenarioThresholdStore(MemoryScenarioThresholdStore inner, ScenarioThresholdCache cache, int hits, int misses) : IScenarioThresholdStore
{
    public MemoryScenarioThresholdStore Inner { get; set; } = inner;
    public ScenarioThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioThreshold? Find(string code)
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

    public void Save(ScenarioThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario thresholds. Empty fields match all.</summary>
public sealed record ScenarioThresholdQuery(string Team, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioThreshold item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<ScenarioThreshold> Apply(List<ScenarioThreshold> items)
    {
        var matched = Collections.Filter(items, (ScenarioThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario threshold store.</summary>
public static class ScenarioThresholdStore
{
    public static IScenarioThresholdStore NewMemoryScenarioThresholdStore()
    {
        return new MemoryScenarioThresholdStore(new Dictionary<string, ScenarioThreshold>(), new List<string>());
    }

    public static IScenarioThresholdStore NewCachedScenarioThresholdStore(MemoryScenarioThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioThresholdStore(inner, ScenarioThresholdCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ScenarioThreshold> FindScenarioThresholds(IScenarioThresholdStore store, ScenarioThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
