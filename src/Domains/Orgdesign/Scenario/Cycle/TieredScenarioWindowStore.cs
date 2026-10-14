namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered scenario windows by code.</summary>
public interface ITieredScenarioWindowStore
{
    TieredScenarioWindow? Find(string code);
    void Save(TieredScenarioWindow item);
    bool Remove(string code);
    List<TieredScenarioWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredScenarioWindowStore(Dictionary<string, TieredScenarioWindow> items, List<string> order) : ITieredScenarioWindowStore
{
    public Dictionary<string, TieredScenarioWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredScenarioWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredScenarioWindow item)
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

    public List<TieredScenarioWindow> List()
    {
        var outValue = new List<TieredScenarioWindow>();
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
public sealed class CachedTieredScenarioWindowStore(MemoryTieredScenarioWindowStore inner, TieredScenarioWindowCache cache, int hits, int misses) : ITieredScenarioWindowStore
{
    public MemoryTieredScenarioWindowStore Inner { get; set; } = inner;
    public TieredScenarioWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredScenarioWindow? Find(string code)
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

    public void Save(TieredScenarioWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredScenarioWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered scenario windows. Empty fields match all.</summary>
public sealed record TieredScenarioWindowQuery(string Unit, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(TieredScenarioWindow item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredScenarioWindow> Apply(List<TieredScenarioWindow> items)
    {
        var matched = Collections.Filter(items, (TieredScenarioWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered scenario window store.</summary>
public static class TieredScenarioWindowStore
{
    public static ITieredScenarioWindowStore NewMemoryTieredScenarioWindowStore()
    {
        return new MemoryTieredScenarioWindowStore(
            new Dictionary<string, TieredScenarioWindow>(),
            new List<string>());
    }

    public static ITieredScenarioWindowStore NewCachedTieredScenarioWindowStore(MemoryTieredScenarioWindowStore inner, int ttl, IClock clock)
    {
        return new CachedTieredScenarioWindowStore(inner, TieredScenarioWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TieredScenarioWindow> FindTieredScenarioWindows(ITieredScenarioWindowStore store, TieredScenarioWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
