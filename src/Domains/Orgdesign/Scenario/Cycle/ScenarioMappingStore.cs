namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario mappings by code.</summary>
public interface IScenarioMappingStore
{
    ScenarioMapping? Find(string code);
    void Save(ScenarioMapping item);
    bool Remove(string code);
    List<ScenarioMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioMappingStore(Dictionary<string, ScenarioMapping> items, List<string> order) : IScenarioMappingStore
{
    public Dictionary<string, ScenarioMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioMapping item)
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

    public List<ScenarioMapping> List()
    {
        var outValue = new List<ScenarioMapping>();
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
public sealed class CachedScenarioMappingStore(MemoryScenarioMappingStore inner, ScenarioMappingCache cache, int hits, int misses) : IScenarioMappingStore
{
    public MemoryScenarioMappingStore Inner { get; set; } = inner;
    public ScenarioMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioMapping? Find(string code)
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

    public void Save(ScenarioMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario mappings. Empty fields match all.</summary>
public sealed record ScenarioMappingQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioMapping item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioMapping> Apply(List<ScenarioMapping> items)
    {
        var matched = Collections.Filter(items, (ScenarioMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario mapping store.</summary>
public static class ScenarioMappingStore
{
    public static IScenarioMappingStore NewMemoryScenarioMappingStore()
    {
        return new MemoryScenarioMappingStore(new Dictionary<string, ScenarioMapping>(), new List<string>());
    }

    public static IScenarioMappingStore NewCachedScenarioMappingStore(MemoryScenarioMappingStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioMappingStore(inner, ScenarioMappingCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ScenarioMapping> FindScenarioMappings(IScenarioMappingStore store, ScenarioMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
