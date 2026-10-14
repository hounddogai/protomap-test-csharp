namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario tiers by code.</summary>
public interface IScenarioTierStore
{
    ScenarioTier? Find(string code);
    void Save(ScenarioTier item);
    bool Remove(string code);
    List<ScenarioTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioTierStore(Dictionary<string, ScenarioTier> items, List<string> order) : IScenarioTierStore
{
    public Dictionary<string, ScenarioTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioTier item)
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

    public List<ScenarioTier> List()
    {
        var outValue = new List<ScenarioTier>();
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
public sealed class CachedScenarioTierStore(MemoryScenarioTierStore inner, ScenarioTierCache cache, int hits, int misses) : IScenarioTierStore
{
    public MemoryScenarioTierStore Inner { get; set; } = inner;
    public ScenarioTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioTier? Find(string code)
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

    public void Save(ScenarioTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario tiers. Empty fields match all.</summary>
public sealed record ScenarioTierQuery(string Unit, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioTier item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioTier> Apply(List<ScenarioTier> items)
    {
        var matched = Collections.Filter(items, (ScenarioTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario tier store.</summary>
public static class ScenarioTierStore
{
    public static IScenarioTierStore NewMemoryScenarioTierStore()
    {
        return new MemoryScenarioTierStore(new Dictionary<string, ScenarioTier>(), new List<string>());
    }

    public static IScenarioTierStore NewCachedScenarioTierStore(MemoryScenarioTierStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioTierStore(inner, ScenarioTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ScenarioTier> FindScenarioTiers(IScenarioTierStore store, ScenarioTierQuery query)
    {
        return query.Apply(store.List());
    }
}
