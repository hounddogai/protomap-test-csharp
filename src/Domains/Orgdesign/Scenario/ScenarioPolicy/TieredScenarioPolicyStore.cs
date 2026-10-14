namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered scenario policies by code.</summary>
public interface ITieredScenarioPolicyStore
{
    TieredScenarioPolicy? Find(string code);
    void Save(TieredScenarioPolicy item);
    bool Remove(string code);
    List<TieredScenarioPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredScenarioPolicyStore(Dictionary<string, TieredScenarioPolicy> items, List<string> order) : ITieredScenarioPolicyStore
{
    public Dictionary<string, TieredScenarioPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredScenarioPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredScenarioPolicy item)
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

    public List<TieredScenarioPolicy> List()
    {
        var outValue = new List<TieredScenarioPolicy>();
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
public sealed class CachedTieredScenarioPolicyStore(MemoryTieredScenarioPolicyStore inner, TieredScenarioPolicyCache cache, int hits, int misses) : ITieredScenarioPolicyStore
{
    public MemoryTieredScenarioPolicyStore Inner { get; set; } = inner;
    public TieredScenarioPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredScenarioPolicy? Find(string code)
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

    public void Save(TieredScenarioPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredScenarioPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered scenario policies. Empty fields match all.</summary>
public sealed record TieredScenarioPolicyQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredScenarioPolicy item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredScenarioPolicy> Apply(List<TieredScenarioPolicy> items)
    {
        var matched = Collections.Filter(items, (TieredScenarioPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered scenario policy store.</summary>
public static class TieredScenarioPolicyStore
{
    public static ITieredScenarioPolicyStore NewMemoryTieredScenarioPolicyStore()
    {
        return new MemoryTieredScenarioPolicyStore(
            new Dictionary<string, TieredScenarioPolicy>(),
            new List<string>());
    }

    public static ITieredScenarioPolicyStore NewCachedTieredScenarioPolicyStore(MemoryTieredScenarioPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedTieredScenarioPolicyStore(inner, TieredScenarioPolicyCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TieredScenarioPolicy> FindTieredScenarioPolicies(ITieredScenarioPolicyStore store, TieredScenarioPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
