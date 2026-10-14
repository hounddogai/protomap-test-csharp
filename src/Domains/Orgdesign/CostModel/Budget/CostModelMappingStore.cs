namespace Acme.Workforce.Orgdesign.CostModel.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model mappings by code.</summary>
public interface ICostModelMappingStore
{
    CostModelMapping? Find(string code);
    void Save(CostModelMapping item);
    bool Remove(string code);
    List<CostModelMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelMappingStore(Dictionary<string, CostModelMapping> items, List<string> order) : ICostModelMappingStore
{
    public Dictionary<string, CostModelMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelMapping item)
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

    public List<CostModelMapping> List()
    {
        var outValue = new List<CostModelMapping>();
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
public sealed class CachedCostModelMappingStore(MemoryCostModelMappingStore inner, CostModelMappingCache cache, int hits, int misses) : ICostModelMappingStore
{
    public MemoryCostModelMappingStore Inner { get; set; } = inner;
    public CostModelMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelMapping? Find(string code)
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

    public void Save(CostModelMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model mappings. Empty fields match all.</summary>
public sealed record CostModelMappingQuery(string Department, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelMapping item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelMapping> Apply(List<CostModelMapping> items)
    {
        var matched = Collections.Filter(items, (CostModelMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model mapping store.</summary>
public static class CostModelMappingStore
{
    public static ICostModelMappingStore NewMemoryCostModelMappingStore()
    {
        return new MemoryCostModelMappingStore(new Dictionary<string, CostModelMapping>(), new List<string>());
    }

    public static ICostModelMappingStore NewCachedCostModelMappingStore(MemoryCostModelMappingStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelMappingStore(inner, CostModelMappingCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CostModelMapping> FindCostModelMappings(ICostModelMappingStore store, CostModelMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
