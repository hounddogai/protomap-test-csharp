namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario bundles by code.</summary>
public interface IScenarioBundleStore
{
    ScenarioBundle? Find(string code);
    void Save(ScenarioBundle item);
    bool Remove(string code);
    List<ScenarioBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioBundleStore(Dictionary<string, ScenarioBundle> items, List<string> order) : IScenarioBundleStore
{
    public Dictionary<string, ScenarioBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioBundle item)
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

    public List<ScenarioBundle> List()
    {
        var outValue = new List<ScenarioBundle>();
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
public sealed class CachedScenarioBundleStore(MemoryScenarioBundleStore inner, ScenarioBundleCache cache, int hits, int misses) : IScenarioBundleStore
{
    public MemoryScenarioBundleStore Inner { get; set; } = inner;
    public ScenarioBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioBundle? Find(string code)
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

    public void Save(ScenarioBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario bundles. Empty fields match all.</summary>
public sealed record ScenarioBundleQuery(string Country, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioBundle item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<ScenarioBundle> Apply(List<ScenarioBundle> items)
    {
        var matched = Collections.Filter(items, (ScenarioBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario bundle store.</summary>
public static class ScenarioBundleStore
{
    public static IScenarioBundleStore NewMemoryScenarioBundleStore()
    {
        return new MemoryScenarioBundleStore(new Dictionary<string, ScenarioBundle>(), new List<string>());
    }

    public static IScenarioBundleStore NewCachedScenarioBundleStore(MemoryScenarioBundleStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioBundleStore(inner, ScenarioBundleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ScenarioBundle> FindScenarioBundles(IScenarioBundleStore store, ScenarioBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
