namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario exceptions by code.</summary>
public interface IScenarioExceptionStore
{
    ScenarioException? Find(string code);
    void Save(ScenarioException item);
    bool Remove(string code);
    List<ScenarioException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioExceptionStore(Dictionary<string, ScenarioException> items, List<string> order) : IScenarioExceptionStore
{
    public Dictionary<string, ScenarioException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioException item)
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

    public List<ScenarioException> List()
    {
        var outValue = new List<ScenarioException>();
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
public sealed class CachedScenarioExceptionStore(MemoryScenarioExceptionStore inner, ScenarioExceptionCache cache, int hits, int misses) : IScenarioExceptionStore
{
    public MemoryScenarioExceptionStore Inner { get; set; } = inner;
    public ScenarioExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioException? Find(string code)
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

    public void Save(ScenarioException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario exceptions. Empty fields match all.</summary>
public sealed record ScenarioExceptionQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioException item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioException> Apply(List<ScenarioException> items)
    {
        var matched = Collections.Filter(items, (ScenarioException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario exception store.</summary>
public static class ScenarioExceptionStore
{
    public static IScenarioExceptionStore NewMemoryScenarioExceptionStore()
    {
        return new MemoryScenarioExceptionStore(new Dictionary<string, ScenarioException>(), new List<string>());
    }

    public static IScenarioExceptionStore NewCachedScenarioExceptionStore(MemoryScenarioExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioExceptionStore(inner, ScenarioExceptionCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ScenarioException> FindScenarioExceptions(IScenarioExceptionStore store, ScenarioExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
