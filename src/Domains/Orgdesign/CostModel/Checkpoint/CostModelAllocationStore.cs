namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model allocations by code.</summary>
public interface ICostModelAllocationStore
{
    CostModelAllocation? Find(string code);
    void Save(CostModelAllocation item);
    bool Remove(string code);
    List<CostModelAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelAllocationStore(Dictionary<string, CostModelAllocation> items, List<string> order) : ICostModelAllocationStore
{
    public Dictionary<string, CostModelAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelAllocation item)
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

    public List<CostModelAllocation> List()
    {
        var outValue = new List<CostModelAllocation>();
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
public sealed class CachedCostModelAllocationStore(MemoryCostModelAllocationStore inner, CostModelAllocationCache cache, int hits, int misses) : ICostModelAllocationStore
{
    public MemoryCostModelAllocationStore Inner { get; set; } = inner;
    public CostModelAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelAllocation? Find(string code)
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

    public void Save(CostModelAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model allocations. Empty fields match all.</summary>
public sealed record CostModelAllocationQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelAllocation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelAllocation> Apply(List<CostModelAllocation> items)
    {
        var matched = Collections.Filter(items, (CostModelAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model allocation store.</summary>
public static class CostModelAllocationStore
{
    public static ICostModelAllocationStore NewMemoryCostModelAllocationStore()
    {
        return new MemoryCostModelAllocationStore(new Dictionary<string, CostModelAllocation>(), new List<string>());
    }

    public static ICostModelAllocationStore NewCachedCostModelAllocationStore(MemoryCostModelAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelAllocationStore(inner, CostModelAllocationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CostModelAllocation> FindCostModelAllocations(ICostModelAllocationStore store, CostModelAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
