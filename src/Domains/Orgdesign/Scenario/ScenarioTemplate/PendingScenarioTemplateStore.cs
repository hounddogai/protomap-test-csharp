namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending scenario templates by code.</summary>
public interface IPendingScenarioTemplateStore
{
    PendingScenarioTemplate? Find(string code);
    void Save(PendingScenarioTemplate item);
    bool Remove(string code);
    List<PendingScenarioTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingScenarioTemplateStore(Dictionary<string, PendingScenarioTemplate> items, List<string> order) : IPendingScenarioTemplateStore
{
    public Dictionary<string, PendingScenarioTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingScenarioTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingScenarioTemplate item)
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

    public List<PendingScenarioTemplate> List()
    {
        var outValue = new List<PendingScenarioTemplate>();
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
public sealed class CachedPendingScenarioTemplateStore(MemoryPendingScenarioTemplateStore inner, PendingScenarioTemplateCache cache, int hits, int misses) : IPendingScenarioTemplateStore
{
    public MemoryPendingScenarioTemplateStore Inner { get; set; } = inner;
    public PendingScenarioTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingScenarioTemplate? Find(string code)
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

    public void Save(PendingScenarioTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingScenarioTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending scenario templates. Empty fields match all.</summary>
public sealed record PendingScenarioTemplateQuery(string Unit, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(PendingScenarioTemplate item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingScenarioTemplate> Apply(List<PendingScenarioTemplate> items)
    {
        var matched = Collections.Filter(items, (PendingScenarioTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending scenario template store.</summary>
public static class PendingScenarioTemplateStore
{
    public static IPendingScenarioTemplateStore NewMemoryPendingScenarioTemplateStore()
    {
        return new MemoryPendingScenarioTemplateStore(
            new Dictionary<string, PendingScenarioTemplate>(),
            new List<string>());
    }

    public static IPendingScenarioTemplateStore NewCachedPendingScenarioTemplateStore(MemoryPendingScenarioTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedPendingScenarioTemplateStore(
            inner,
            PendingScenarioTemplateCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PendingScenarioTemplate> FindPendingScenarioTemplates(IPendingScenarioTemplateStore store, PendingScenarioTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
