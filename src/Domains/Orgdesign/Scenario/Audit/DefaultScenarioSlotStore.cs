namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default scenario slots by code.</summary>
public interface IDefaultScenarioSlotStore
{
    DefaultScenarioSlot? Find(string code);
    void Save(DefaultScenarioSlot item);
    bool Remove(string code);
    List<DefaultScenarioSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultScenarioSlotStore(Dictionary<string, DefaultScenarioSlot> items, List<string> order) : IDefaultScenarioSlotStore
{
    public Dictionary<string, DefaultScenarioSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultScenarioSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultScenarioSlot item)
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

    public List<DefaultScenarioSlot> List()
    {
        var outValue = new List<DefaultScenarioSlot>();
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
public sealed class CachedDefaultScenarioSlotStore(MemoryDefaultScenarioSlotStore inner, DefaultScenarioSlotCache cache, int hits, int misses) : IDefaultScenarioSlotStore
{
    public MemoryDefaultScenarioSlotStore Inner { get; set; } = inner;
    public DefaultScenarioSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultScenarioSlot? Find(string code)
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

    public void Save(DefaultScenarioSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultScenarioSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default scenario slots. Empty fields match all.</summary>
public sealed record DefaultScenarioSlotQuery(string Unit, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultScenarioSlot item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultScenarioSlot> Apply(List<DefaultScenarioSlot> items)
    {
        var matched = Collections.Filter(items, (DefaultScenarioSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default scenario slot store.</summary>
public static class DefaultScenarioSlotStore
{
    public static IDefaultScenarioSlotStore NewMemoryDefaultScenarioSlotStore()
    {
        return new MemoryDefaultScenarioSlotStore(new Dictionary<string, DefaultScenarioSlot>(), new List<string>());
    }

    public static IDefaultScenarioSlotStore NewCachedDefaultScenarioSlotStore(MemoryDefaultScenarioSlotStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultScenarioSlotStore(inner, DefaultScenarioSlotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<DefaultScenarioSlot> FindDefaultScenarioSlots(IDefaultScenarioSlotStore store, DefaultScenarioSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
