namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model groups by code.</summary>
public interface ICostModelGroupStore
{
    CostModelGroup? Find(string code);
    void Save(CostModelGroup item);
    bool Remove(string code);
    List<CostModelGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelGroupStore(Dictionary<string, CostModelGroup> items, List<string> order) : ICostModelGroupStore
{
    public Dictionary<string, CostModelGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelGroup item)
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

    public List<CostModelGroup> List()
    {
        var outValue = new List<CostModelGroup>();
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
public sealed class CachedCostModelGroupStore(MemoryCostModelGroupStore inner, CostModelGroupCache cache, int hits, int misses) : ICostModelGroupStore
{
    public MemoryCostModelGroupStore Inner { get; set; } = inner;
    public CostModelGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelGroup? Find(string code)
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

    public void Save(CostModelGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model groups. Empty fields match all.</summary>
public sealed record CostModelGroupQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<CostModelGroup> Apply(List<CostModelGroup> items)
    {
        var matched = Collections.Filter(items, (CostModelGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model group store.</summary>
public static class CostModelGroupStore
{
    public static ICostModelGroupStore NewMemoryCostModelGroupStore()
    {
        return new MemoryCostModelGroupStore(new Dictionary<string, CostModelGroup>(), new List<string>());
    }

    public static ICostModelGroupStore NewCachedCostModelGroupStore(MemoryCostModelGroupStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelGroupStore(inner, CostModelGroupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CostModelGroup> FindCostModelGroups(ICostModelGroupStore store, CostModelGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
