namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario batches by code.</summary>
public interface IScenarioBatchStore
{
    ScenarioBatch? Find(string code);
    void Save(ScenarioBatch item);
    bool Remove(string code);
    List<ScenarioBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioBatchStore(Dictionary<string, ScenarioBatch> items, List<string> order) : IScenarioBatchStore
{
    public Dictionary<string, ScenarioBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioBatch item)
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

    public List<ScenarioBatch> List()
    {
        var outValue = new List<ScenarioBatch>();
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
public sealed class CachedScenarioBatchStore(MemoryScenarioBatchStore inner, ScenarioBatchCache cache, int hits, int misses) : IScenarioBatchStore
{
    public MemoryScenarioBatchStore Inner { get; set; } = inner;
    public ScenarioBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioBatch? Find(string code)
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

    public void Save(ScenarioBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario batches. Empty fields match all.</summary>
public sealed record ScenarioBatchQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioBatch item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioBatch> Apply(List<ScenarioBatch> items)
    {
        var matched = Collections.Filter(items, (ScenarioBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario batch store.</summary>
public static class ScenarioBatchStore
{
    public static IScenarioBatchStore NewMemoryScenarioBatchStore()
    {
        return new MemoryScenarioBatchStore(new Dictionary<string, ScenarioBatch>(), new List<string>());
    }

    public static IScenarioBatchStore NewCachedScenarioBatchStore(MemoryScenarioBatchStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioBatchStore(inner, ScenarioBatchCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ScenarioBatch> FindScenarioBatches(IScenarioBatchStore store, ScenarioBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
