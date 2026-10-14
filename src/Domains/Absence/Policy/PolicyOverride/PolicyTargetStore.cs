namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy targets by code.</summary>
public interface IPolicyTargetStore
{
    PolicyTarget? Find(string code);
    void Save(PolicyTarget item);
    bool Remove(string code);
    List<PolicyTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyTargetStore(Dictionary<string, PolicyTarget> items, List<string> order) : IPolicyTargetStore
{
    public Dictionary<string, PolicyTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyTarget item)
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

    public List<PolicyTarget> List()
    {
        var outValue = new List<PolicyTarget>();
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
public sealed class CachedPolicyTargetStore(MemoryPolicyTargetStore inner, PolicyTargetCache cache, int hits, int misses) : IPolicyTargetStore
{
    public MemoryPolicyTargetStore Inner { get; set; } = inner;
    public PolicyTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyTarget? Find(string code)
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

    public void Save(PolicyTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy targets. Empty fields match all.</summary>
public sealed record PolicyTargetQuery(string Category, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyTarget item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<PolicyTarget> Apply(List<PolicyTarget> items)
    {
        var matched = Collections.Filter(items, (PolicyTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy target store.</summary>
public static class PolicyTargetStore
{
    public static IPolicyTargetStore NewMemoryPolicyTargetStore()
    {
        return new MemoryPolicyTargetStore(new Dictionary<string, PolicyTarget>(), new List<string>());
    }

    public static IPolicyTargetStore NewCachedPolicyTargetStore(MemoryPolicyTargetStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyTargetStore(inner, PolicyTargetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PolicyTarget> FindPolicyTargets(IPolicyTargetStore store, PolicyTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
