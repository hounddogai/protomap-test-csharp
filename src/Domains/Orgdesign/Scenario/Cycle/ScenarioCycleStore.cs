namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario cycles by code.</summary>
public interface IScenarioCycleStore
{
    ScenarioCycle? Find(string code);
    void Save(ScenarioCycle item);
    bool Remove(string code);
    List<ScenarioCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioCycleStore(Dictionary<string, ScenarioCycle> items, List<string> order) : IScenarioCycleStore
{
    public Dictionary<string, ScenarioCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioCycle item)
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

    public List<ScenarioCycle> List()
    {
        var outValue = new List<ScenarioCycle>();
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
public sealed class CachedScenarioCycleStore(MemoryScenarioCycleStore inner, ScenarioCycleCache cache, int hits, int misses) : IScenarioCycleStore
{
    public MemoryScenarioCycleStore Inner { get; set; } = inner;
    public ScenarioCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioCycle? Find(string code)
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

    public void Save(ScenarioCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario cycles. Empty fields match all.</summary>
public sealed record ScenarioCycleQuery(string Country, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioCycle item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioCycle> Apply(List<ScenarioCycle> items)
    {
        var matched = Collections.Filter(items, (ScenarioCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario cycle store.</summary>
public static class ScenarioCycleStore
{
    public static IScenarioCycleStore NewMemoryScenarioCycleStore()
    {
        return new MemoryScenarioCycleStore(new Dictionary<string, ScenarioCycle>(), new List<string>());
    }

    public static IScenarioCycleStore NewCachedScenarioCycleStore(MemoryScenarioCycleStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioCycleStore(inner, ScenarioCycleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ScenarioCycle> FindScenarioCycles(IScenarioCycleStore store, ScenarioCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
