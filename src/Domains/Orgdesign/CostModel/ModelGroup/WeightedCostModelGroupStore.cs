namespace Acme.Workforce.Orgdesign.CostModel.ModelGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted cost model groups by code.</summary>
public interface IWeightedCostModelGroupStore
{
    WeightedCostModelGroup? Find(string code);
    void Save(WeightedCostModelGroup item);
    bool Remove(string code);
    List<WeightedCostModelGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedCostModelGroupStore(Dictionary<string, WeightedCostModelGroup> items, List<string> order) : IWeightedCostModelGroupStore
{
    public Dictionary<string, WeightedCostModelGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedCostModelGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedCostModelGroup item)
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

    public List<WeightedCostModelGroup> List()
    {
        var outValue = new List<WeightedCostModelGroup>();
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
public sealed class CachedWeightedCostModelGroupStore(MemoryWeightedCostModelGroupStore inner, WeightedCostModelGroupCache cache, int hits, int misses) : IWeightedCostModelGroupStore
{
    public MemoryWeightedCostModelGroupStore Inner { get; set; } = inner;
    public WeightedCostModelGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedCostModelGroup? Find(string code)
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

    public void Save(WeightedCostModelGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedCostModelGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted cost model groups. Empty fields match all.</summary>
public sealed record WeightedCostModelGroupQuery(string Country, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedCostModelGroup item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedCostModelGroup> Apply(List<WeightedCostModelGroup> items)
    {
        var matched = Collections.Filter(items, (WeightedCostModelGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted cost model group store.</summary>
public static class WeightedCostModelGroupStore
{
    public static IWeightedCostModelGroupStore NewMemoryWeightedCostModelGroupStore()
    {
        return new MemoryWeightedCostModelGroupStore(
            new Dictionary<string, WeightedCostModelGroup>(),
            new List<string>());
    }

    public static IWeightedCostModelGroupStore NewCachedWeightedCostModelGroupStore(MemoryWeightedCostModelGroupStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedCostModelGroupStore(
            inner,
            WeightedCostModelGroupCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedCostModelGroup> FindWeightedCostModelGroups(IWeightedCostModelGroupStore store, WeightedCostModelGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
