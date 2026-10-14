namespace Acme.Workforce.Talent.SuccessionPlan.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive succession plan policies by code.</summary>
public interface IAdaptiveSuccessionPlanPolicyStore
{
    AdaptiveSuccessionPlanPolicy? Find(string code);
    void Save(AdaptiveSuccessionPlanPolicy item);
    bool Remove(string code);
    List<AdaptiveSuccessionPlanPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveSuccessionPlanPolicyStore(Dictionary<string, AdaptiveSuccessionPlanPolicy> items, List<string> order) : IAdaptiveSuccessionPlanPolicyStore
{
    public Dictionary<string, AdaptiveSuccessionPlanPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveSuccessionPlanPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveSuccessionPlanPolicy item)
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

    public List<AdaptiveSuccessionPlanPolicy> List()
    {
        var outValue = new List<AdaptiveSuccessionPlanPolicy>();
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
public sealed class CachedAdaptiveSuccessionPlanPolicyStore(MemoryAdaptiveSuccessionPlanPolicyStore inner, AdaptiveSuccessionPlanPolicyCache cache, int hits, int misses) : IAdaptiveSuccessionPlanPolicyStore
{
    public MemoryAdaptiveSuccessionPlanPolicyStore Inner { get; set; } = inner;
    public AdaptiveSuccessionPlanPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveSuccessionPlanPolicy? Find(string code)
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

    public void Save(AdaptiveSuccessionPlanPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveSuccessionPlanPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive succession plan policies. Empty fields match all.</summary>
public sealed record AdaptiveSuccessionPlanPolicyQuery(string Name, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveSuccessionPlanPolicy item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveSuccessionPlanPolicy> Apply(List<AdaptiveSuccessionPlanPolicy> items)
    {
        var matched = Collections.Filter(items, (AdaptiveSuccessionPlanPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive succession plan policy store.</summary>
public static class AdaptiveSuccessionPlanPolicyStore
{
    public static IAdaptiveSuccessionPlanPolicyStore NewMemoryAdaptiveSuccessionPlanPolicyStore()
    {
        return new MemoryAdaptiveSuccessionPlanPolicyStore(
            new Dictionary<string, AdaptiveSuccessionPlanPolicy>(),
            new List<string>());
    }

    public static IAdaptiveSuccessionPlanPolicyStore NewCachedAdaptiveSuccessionPlanPolicyStore(MemoryAdaptiveSuccessionPlanPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveSuccessionPlanPolicyStore(
            inner,
            AdaptiveSuccessionPlanPolicyCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveSuccessionPlanPolicy> FindAdaptiveSuccessionPlanPolicies(IAdaptiveSuccessionPlanPolicyStore store, AdaptiveSuccessionPlanPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
