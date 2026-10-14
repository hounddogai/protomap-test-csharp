namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy succession plan policies by code.</summary>
public interface ILegacySuccessionPlanPolicyStore
{
    LegacySuccessionPlanPolicy? Find(string code);
    void Save(LegacySuccessionPlanPolicy item);
    bool Remove(string code);
    List<LegacySuccessionPlanPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacySuccessionPlanPolicyStore(Dictionary<string, LegacySuccessionPlanPolicy> items, List<string> order) : ILegacySuccessionPlanPolicyStore
{
    public Dictionary<string, LegacySuccessionPlanPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacySuccessionPlanPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacySuccessionPlanPolicy item)
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

    public List<LegacySuccessionPlanPolicy> List()
    {
        var outValue = new List<LegacySuccessionPlanPolicy>();
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
public sealed class CachedLegacySuccessionPlanPolicyStore(MemoryLegacySuccessionPlanPolicyStore inner, LegacySuccessionPlanPolicyCache cache, int hits, int misses) : ILegacySuccessionPlanPolicyStore
{
    public MemoryLegacySuccessionPlanPolicyStore Inner { get; set; } = inner;
    public LegacySuccessionPlanPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacySuccessionPlanPolicy? Find(string code)
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

    public void Save(LegacySuccessionPlanPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacySuccessionPlanPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy succession plan policies. Empty fields match all.</summary>
public sealed record LegacySuccessionPlanPolicyQuery(string Country, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LegacySuccessionPlanPolicy item)
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

    public List<LegacySuccessionPlanPolicy> Apply(List<LegacySuccessionPlanPolicy> items)
    {
        var matched = Collections.Filter(items, (LegacySuccessionPlanPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy succession plan policy store.</summary>
public static class LegacySuccessionPlanPolicyStore
{
    public static ILegacySuccessionPlanPolicyStore NewMemoryLegacySuccessionPlanPolicyStore()
    {
        return new MemoryLegacySuccessionPlanPolicyStore(
            new Dictionary<string, LegacySuccessionPlanPolicy>(),
            new List<string>());
    }

    public static ILegacySuccessionPlanPolicyStore NewCachedLegacySuccessionPlanPolicyStore(MemoryLegacySuccessionPlanPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedLegacySuccessionPlanPolicyStore(
            inner,
            LegacySuccessionPlanPolicyCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacySuccessionPlanPolicy> FindLegacySuccessionPlanPolicies(ILegacySuccessionPlanPolicyStore store, LegacySuccessionPlanPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
