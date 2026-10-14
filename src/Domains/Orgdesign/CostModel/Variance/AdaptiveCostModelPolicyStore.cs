namespace Acme.Workforce.Orgdesign.CostModel.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive cost model policies by code.</summary>
public interface IAdaptiveCostModelPolicyStore
{
    AdaptiveCostModelPolicy? Find(string code);
    void Save(AdaptiveCostModelPolicy item);
    bool Remove(string code);
    List<AdaptiveCostModelPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveCostModelPolicyStore(Dictionary<string, AdaptiveCostModelPolicy> items, List<string> order) : IAdaptiveCostModelPolicyStore
{
    public Dictionary<string, AdaptiveCostModelPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveCostModelPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveCostModelPolicy item)
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

    public List<AdaptiveCostModelPolicy> List()
    {
        var outValue = new List<AdaptiveCostModelPolicy>();
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
public sealed class CachedAdaptiveCostModelPolicyStore(MemoryAdaptiveCostModelPolicyStore inner, AdaptiveCostModelPolicyCache cache, int hits, int misses) : IAdaptiveCostModelPolicyStore
{
    public MemoryAdaptiveCostModelPolicyStore Inner { get; set; } = inner;
    public AdaptiveCostModelPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveCostModelPolicy? Find(string code)
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

    public void Save(AdaptiveCostModelPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveCostModelPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive cost model policies. Empty fields match all.</summary>
public sealed record AdaptiveCostModelPolicyQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveCostModelPolicy item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveCostModelPolicy> Apply(List<AdaptiveCostModelPolicy> items)
    {
        var matched = Collections.Filter(items, (AdaptiveCostModelPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive cost model policy store.</summary>
public static class AdaptiveCostModelPolicyStore
{
    public static IAdaptiveCostModelPolicyStore NewMemoryAdaptiveCostModelPolicyStore()
    {
        return new MemoryAdaptiveCostModelPolicyStore(
            new Dictionary<string, AdaptiveCostModelPolicy>(),
            new List<string>());
    }

    public static IAdaptiveCostModelPolicyStore NewCachedAdaptiveCostModelPolicyStore(MemoryAdaptiveCostModelPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveCostModelPolicyStore(
            inner,
            AdaptiveCostModelPolicyCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveCostModelPolicy> FindAdaptiveCostModelPolicies(IAdaptiveCostModelPolicyStore store, AdaptiveCostModelPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
