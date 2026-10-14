namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario templates by code.</summary>
public interface IScenarioTemplateStore
{
    ScenarioTemplate? Find(string code);
    void Save(ScenarioTemplate item);
    bool Remove(string code);
    List<ScenarioTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioTemplateStore(Dictionary<string, ScenarioTemplate> items, List<string> order) : IScenarioTemplateStore
{
    public Dictionary<string, ScenarioTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioTemplate item)
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

    public List<ScenarioTemplate> List()
    {
        var outValue = new List<ScenarioTemplate>();
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
public sealed class CachedScenarioTemplateStore(MemoryScenarioTemplateStore inner, ScenarioTemplateCache cache, int hits, int misses) : IScenarioTemplateStore
{
    public MemoryScenarioTemplateStore Inner { get; set; } = inner;
    public ScenarioTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioTemplate? Find(string code)
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

    public void Save(ScenarioTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario templates. Empty fields match all.</summary>
public sealed record ScenarioTemplateQuery(string Label, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioTemplate item)
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

    public List<ScenarioTemplate> Apply(List<ScenarioTemplate> items)
    {
        var matched = Collections.Filter(items, (ScenarioTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario template store.</summary>
public static class ScenarioTemplateStore
{
    public static IScenarioTemplateStore NewMemoryScenarioTemplateStore()
    {
        return new MemoryScenarioTemplateStore(new Dictionary<string, ScenarioTemplate>(), new List<string>());
    }

    public static IScenarioTemplateStore NewCachedScenarioTemplateStore(MemoryScenarioTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioTemplateStore(inner, ScenarioTemplateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ScenarioTemplate> FindScenarioTemplates(IScenarioTemplateStore store, ScenarioTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
