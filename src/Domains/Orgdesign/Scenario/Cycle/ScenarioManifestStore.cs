namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario manifests by code.</summary>
public interface IScenarioManifestStore
{
    ScenarioManifest? Find(string code);
    void Save(ScenarioManifest item);
    bool Remove(string code);
    List<ScenarioManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioManifestStore(Dictionary<string, ScenarioManifest> items, List<string> order) : IScenarioManifestStore
{
    public Dictionary<string, ScenarioManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioManifest item)
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

    public List<ScenarioManifest> List()
    {
        var outValue = new List<ScenarioManifest>();
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
public sealed class CachedScenarioManifestStore(MemoryScenarioManifestStore inner, ScenarioManifestCache cache, int hits, int misses) : IScenarioManifestStore
{
    public MemoryScenarioManifestStore Inner { get; set; } = inner;
    public ScenarioManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioManifest? Find(string code)
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

    public void Save(ScenarioManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario manifests. Empty fields match all.</summary>
public sealed record ScenarioManifestQuery(string Locale, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioManifest item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioManifest> Apply(List<ScenarioManifest> items)
    {
        var matched = Collections.Filter(items, (ScenarioManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario manifest store.</summary>
public static class ScenarioManifestStore
{
    public static IScenarioManifestStore NewMemoryScenarioManifestStore()
    {
        return new MemoryScenarioManifestStore(new Dictionary<string, ScenarioManifest>(), new List<string>());
    }

    public static IScenarioManifestStore NewCachedScenarioManifestStore(MemoryScenarioManifestStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioManifestStore(inner, ScenarioManifestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ScenarioManifest> FindScenarioManifests(IScenarioManifestStore store, ScenarioManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
