namespace Acme.Workforce.Orgdesign.Scenario.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario slots by code.</summary>
public interface IScenarioSlotStore
{
    ScenarioSlot? Find(string code);
    void Save(ScenarioSlot item);
    bool Remove(string code);
    List<ScenarioSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioSlotStore(Dictionary<string, ScenarioSlot> items, List<string> order) : IScenarioSlotStore
{
    public Dictionary<string, ScenarioSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioSlot item)
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

    public List<ScenarioSlot> List()
    {
        var outValue = new List<ScenarioSlot>();
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
public sealed class CachedScenarioSlotStore(MemoryScenarioSlotStore inner, ScenarioSlotCache cache, int hits, int misses) : IScenarioSlotStore
{
    public MemoryScenarioSlotStore Inner { get; set; } = inner;
    public ScenarioSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioSlot? Find(string code)
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

    public void Save(ScenarioSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario slots. Empty fields match all.</summary>
public sealed record ScenarioSlotQuery(string Unit, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioSlot item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<ScenarioSlot> Apply(List<ScenarioSlot> items)
    {
        var matched = Collections.Filter(items, (ScenarioSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario slot store.</summary>
public static class ScenarioSlotStore
{
    public static IScenarioSlotStore NewMemoryScenarioSlotStore()
    {
        return new MemoryScenarioSlotStore(new Dictionary<string, ScenarioSlot>(), new List<string>());
    }

    public static IScenarioSlotStore NewCachedScenarioSlotStore(MemoryScenarioSlotStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioSlotStore(inner, ScenarioSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ScenarioSlot> FindScenarioSlots(IScenarioSlotStore store, ScenarioSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
