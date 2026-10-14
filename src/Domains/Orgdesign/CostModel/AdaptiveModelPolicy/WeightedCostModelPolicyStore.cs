namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted cost model policies by code.</summary>
public interface IWeightedCostModelPolicyStore
{
    WeightedCostModelPolicy? Find(string code);
    void Save(WeightedCostModelPolicy item);
    bool Remove(string code);
    List<WeightedCostModelPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedCostModelPolicyStore(Dictionary<string, WeightedCostModelPolicy> items, List<string> order) : IWeightedCostModelPolicyStore
{
    public Dictionary<string, WeightedCostModelPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedCostModelPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedCostModelPolicy item)
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

    public List<WeightedCostModelPolicy> List()
    {
        var outValue = new List<WeightedCostModelPolicy>();
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
public sealed class CachedWeightedCostModelPolicyStore(MemoryWeightedCostModelPolicyStore inner, WeightedCostModelPolicyCache cache, int hits, int misses) : IWeightedCostModelPolicyStore
{
    public MemoryWeightedCostModelPolicyStore Inner { get; set; } = inner;
    public WeightedCostModelPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedCostModelPolicy? Find(string code)
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

    public void Save(WeightedCostModelPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedCostModelPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted cost model policies. Empty fields match all.</summary>
public sealed record WeightedCostModelPolicyQuery(string Segment, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedCostModelPolicy item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedCostModelPolicy> Apply(List<WeightedCostModelPolicy> items)
    {
        var matched = Collections.Filter(items, (WeightedCostModelPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted cost model policy store.</summary>
public static class WeightedCostModelPolicyStore
{
    public static IWeightedCostModelPolicyStore NewMemoryWeightedCostModelPolicyStore()
    {
        return new MemoryWeightedCostModelPolicyStore(
            new Dictionary<string, WeightedCostModelPolicy>(),
            new List<string>());
    }

    public static IWeightedCostModelPolicyStore NewCachedWeightedCostModelPolicyStore(MemoryWeightedCostModelPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedCostModelPolicyStore(
            inner,
            WeightedCostModelPolicyCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedCostModelPolicy> FindWeightedCostModelPolicies(IWeightedCostModelPolicyStore store, WeightedCostModelPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
