namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan policies by code.</summary>
public interface ISuccessionPlanPolicyStore
{
    SuccessionPlanPolicy? Find(string code);
    void Save(SuccessionPlanPolicy item);
    bool Remove(string code);
    List<SuccessionPlanPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanPolicyStore(Dictionary<string, SuccessionPlanPolicy> items, List<string> order) : ISuccessionPlanPolicyStore
{
    public Dictionary<string, SuccessionPlanPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanPolicy item)
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

    public List<SuccessionPlanPolicy> List()
    {
        var outValue = new List<SuccessionPlanPolicy>();
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
public sealed class CachedSuccessionPlanPolicyStore(MemorySuccessionPlanPolicyStore inner, SuccessionPlanPolicyCache cache, int hits, int misses) : ISuccessionPlanPolicyStore
{
    public MemorySuccessionPlanPolicyStore Inner { get; set; } = inner;
    public SuccessionPlanPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanPolicy? Find(string code)
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

    public void Save(SuccessionPlanPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan policies. Empty fields match all.</summary>
public sealed record SuccessionPlanPolicyQuery(string Region, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanPolicy item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanPolicy> Apply(List<SuccessionPlanPolicy> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan policy store.</summary>
public static class SuccessionPlanPolicyStore
{
    public static ISuccessionPlanPolicyStore NewMemorySuccessionPlanPolicyStore()
    {
        return new MemorySuccessionPlanPolicyStore(
            new Dictionary<string, SuccessionPlanPolicy>(),
            new List<string>());
    }

    public static ISuccessionPlanPolicyStore NewCachedSuccessionPlanPolicyStore(MemorySuccessionPlanPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanPolicyStore(inner, SuccessionPlanPolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanPolicy> FindSuccessionPlanPolicies(ISuccessionPlanPolicyStore store, SuccessionPlanPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
