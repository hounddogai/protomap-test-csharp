namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps cost model policies by code.</summary>
public interface ICostModelPolicyStore
{
    CostModelPolicy? Find(string code);
    void Save(CostModelPolicy item);
    bool Remove(string code);
    List<CostModelPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCostModelPolicyStore(Dictionary<string, CostModelPolicy> items, List<string> order) : ICostModelPolicyStore
{
    public Dictionary<string, CostModelPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CostModelPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CostModelPolicy item)
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

    public List<CostModelPolicy> List()
    {
        var outValue = new List<CostModelPolicy>();
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
public sealed class CachedCostModelPolicyStore(MemoryCostModelPolicyStore inner, CostModelPolicyCache cache, int hits, int misses) : ICostModelPolicyStore
{
    public MemoryCostModelPolicyStore Inner { get; set; } = inner;
    public CostModelPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CostModelPolicy? Find(string code)
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

    public void Save(CostModelPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CostModelPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects cost model policies. Empty fields match all.</summary>
public sealed record CostModelPolicyQuery(string Unit, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(CostModelPolicy item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CostModelPolicy> Apply(List<CostModelPolicy> items)
    {
        var matched = Collections.Filter(items, (CostModelPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of cost model policy store.</summary>
public static class CostModelPolicyStore
{
    public static ICostModelPolicyStore NewMemoryCostModelPolicyStore()
    {
        return new MemoryCostModelPolicyStore(new Dictionary<string, CostModelPolicy>(), new List<string>());
    }

    public static ICostModelPolicyStore NewCachedCostModelPolicyStore(MemoryCostModelPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedCostModelPolicyStore(inner, CostModelPolicyCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CostModelPolicy> FindCostModelPolicies(ICostModelPolicyStore store, CostModelPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
