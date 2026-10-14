namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack overrides by code.</summary>
public interface IPolicyAckOverrideStore
{
    PolicyAckOverride? Find(string code);
    void Save(PolicyAckOverride item);
    bool Remove(string code);
    List<PolicyAckOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckOverrideStore(Dictionary<string, PolicyAckOverride> items, List<string> order) : IPolicyAckOverrideStore
{
    public Dictionary<string, PolicyAckOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckOverride item)
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

    public List<PolicyAckOverride> List()
    {
        var outValue = new List<PolicyAckOverride>();
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
public sealed class CachedPolicyAckOverrideStore(MemoryPolicyAckOverrideStore inner, PolicyAckOverrideCache cache, int hits, int misses) : IPolicyAckOverrideStore
{
    public MemoryPolicyAckOverrideStore Inner { get; set; } = inner;
    public PolicyAckOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckOverride? Find(string code)
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

    public void Save(PolicyAckOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack overrides. Empty fields match all.</summary>
public sealed record PolicyAckOverrideQuery(string Channel, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckOverride item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckOverride> Apply(List<PolicyAckOverride> items)
    {
        var matched = Collections.Filter(items, (PolicyAckOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack override store.</summary>
public static class PolicyAckOverrideStore
{
    public static IPolicyAckOverrideStore NewMemoryPolicyAckOverrideStore()
    {
        return new MemoryPolicyAckOverrideStore(new Dictionary<string, PolicyAckOverride>(), new List<string>());
    }

    public static IPolicyAckOverrideStore NewCachedPolicyAckOverrideStore(MemoryPolicyAckOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckOverrideStore(inner, PolicyAckOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PolicyAckOverride> FindPolicyAckOverrides(IPolicyAckOverrideStore store, PolicyAckOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
