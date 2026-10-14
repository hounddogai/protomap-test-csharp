namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario scores by code.</summary>
public interface IScenarioScoreStore
{
    ScenarioScore? Find(string code);
    void Save(ScenarioScore item);
    bool Remove(string code);
    List<ScenarioScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioScoreStore(Dictionary<string, ScenarioScore> items, List<string> order) : IScenarioScoreStore
{
    public Dictionary<string, ScenarioScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioScore item)
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

    public List<ScenarioScore> List()
    {
        var outValue = new List<ScenarioScore>();
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
public sealed class CachedScenarioScoreStore(MemoryScenarioScoreStore inner, ScenarioScoreCache cache, int hits, int misses) : IScenarioScoreStore
{
    public MemoryScenarioScoreStore Inner { get; set; } = inner;
    public ScenarioScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioScore? Find(string code)
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

    public void Save(ScenarioScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario scores. Empty fields match all.</summary>
public sealed record ScenarioScoreQuery(string Channel, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioScore item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<ScenarioScore> Apply(List<ScenarioScore> items)
    {
        var matched = Collections.Filter(items, (ScenarioScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario score store.</summary>
public static class ScenarioScoreStore
{
    public static IScenarioScoreStore NewMemoryScenarioScoreStore()
    {
        return new MemoryScenarioScoreStore(new Dictionary<string, ScenarioScore>(), new List<string>());
    }

    public static IScenarioScoreStore NewCachedScenarioScoreStore(MemoryScenarioScoreStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioScoreStore(inner, ScenarioScoreCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ScenarioScore> FindScenarioScores(IScenarioScoreStore store, ScenarioScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
