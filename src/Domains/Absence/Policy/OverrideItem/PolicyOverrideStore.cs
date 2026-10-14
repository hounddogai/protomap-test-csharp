namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy overrides by code.</summary>
public interface IPolicyOverrideStore
{
    PolicyOverride? Find(string code);
    void Save(PolicyOverride item);
    bool Remove(string code);
    List<PolicyOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyOverrideStore(Dictionary<string, PolicyOverride> items, List<string> order) : IPolicyOverrideStore
{
    public Dictionary<string, PolicyOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyOverride item)
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

    public List<PolicyOverride> List()
    {
        var outValue = new List<PolicyOverride>();
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
public sealed class CachedPolicyOverrideStore(MemoryPolicyOverrideStore inner, PolicyOverrideCache cache, int hits, int misses) : IPolicyOverrideStore
{
    public MemoryPolicyOverrideStore Inner { get; set; } = inner;
    public PolicyOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyOverride? Find(string code)
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

    public void Save(PolicyOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy overrides. Empty fields match all.</summary>
public sealed record PolicyOverrideQuery(string Region, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyOverride item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyOverride> Apply(List<PolicyOverride> items)
    {
        var matched = Collections.Filter(items, (PolicyOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy override store.</summary>
public static class PolicyOverrideStore
{
    public static IPolicyOverrideStore NewMemoryPolicyOverrideStore()
    {
        return new MemoryPolicyOverrideStore(new Dictionary<string, PolicyOverride>(), new List<string>());
    }

    public static IPolicyOverrideStore NewCachedPolicyOverrideStore(MemoryPolicyOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyOverrideStore(inner, PolicyOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PolicyOverride> FindPolicyOverrides(IPolicyOverrideStore store, PolicyOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
