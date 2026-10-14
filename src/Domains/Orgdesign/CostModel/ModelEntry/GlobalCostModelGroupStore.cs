namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global cost model groups by code.</summary>
public interface IGlobalCostModelGroupStore
{
    GlobalCostModelGroup? Find(string code);
    void Save(GlobalCostModelGroup item);
    bool Remove(string code);
    List<GlobalCostModelGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalCostModelGroupStore(Dictionary<string, GlobalCostModelGroup> items, List<string> order) : IGlobalCostModelGroupStore
{
    public Dictionary<string, GlobalCostModelGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalCostModelGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalCostModelGroup item)
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

    public List<GlobalCostModelGroup> List()
    {
        var outValue = new List<GlobalCostModelGroup>();
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
public sealed class CachedGlobalCostModelGroupStore(MemoryGlobalCostModelGroupStore inner, GlobalCostModelGroupCache cache, int hits, int misses) : IGlobalCostModelGroupStore
{
    public MemoryGlobalCostModelGroupStore Inner { get; set; } = inner;
    public GlobalCostModelGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalCostModelGroup? Find(string code)
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

    public void Save(GlobalCostModelGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalCostModelGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global cost model groups. Empty fields match all.</summary>
public sealed record GlobalCostModelGroupQuery(string Category, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalCostModelGroup item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalCostModelGroup> Apply(List<GlobalCostModelGroup> items)
    {
        var matched = Collections.Filter(items, (GlobalCostModelGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global cost model group store.</summary>
public static class GlobalCostModelGroupStore
{
    public static IGlobalCostModelGroupStore NewMemoryGlobalCostModelGroupStore()
    {
        return new MemoryGlobalCostModelGroupStore(
            new Dictionary<string, GlobalCostModelGroup>(),
            new List<string>());
    }

    public static IGlobalCostModelGroupStore NewCachedGlobalCostModelGroupStore(MemoryGlobalCostModelGroupStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalCostModelGroupStore(inner, GlobalCostModelGroupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<GlobalCostModelGroup> FindGlobalCostModelGroups(IGlobalCostModelGroupStore store, GlobalCostModelGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
