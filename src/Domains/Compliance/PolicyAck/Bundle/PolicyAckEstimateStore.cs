namespace Acme.Workforce.Compliance.PolicyAck.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack estimates by code.</summary>
public interface IPolicyAckEstimateStore
{
    PolicyAckEstimate? Find(string code);
    void Save(PolicyAckEstimate item);
    bool Remove(string code);
    List<PolicyAckEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckEstimateStore(Dictionary<string, PolicyAckEstimate> items, List<string> order) : IPolicyAckEstimateStore
{
    public Dictionary<string, PolicyAckEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckEstimate item)
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

    public List<PolicyAckEstimate> List()
    {
        var outValue = new List<PolicyAckEstimate>();
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
public sealed class CachedPolicyAckEstimateStore(MemoryPolicyAckEstimateStore inner, PolicyAckEstimateCache cache, int hits, int misses) : IPolicyAckEstimateStore
{
    public MemoryPolicyAckEstimateStore Inner { get; set; } = inner;
    public PolicyAckEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckEstimate? Find(string code)
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

    public void Save(PolicyAckEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack estimates. Empty fields match all.</summary>
public sealed record PolicyAckEstimateQuery(string Region, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckEstimate item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<PolicyAckEstimate> Apply(List<PolicyAckEstimate> items)
    {
        var matched = Collections.Filter(items, (PolicyAckEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack estimate store.</summary>
public static class PolicyAckEstimateStore
{
    public static IPolicyAckEstimateStore NewMemoryPolicyAckEstimateStore()
    {
        return new MemoryPolicyAckEstimateStore(new Dictionary<string, PolicyAckEstimate>(), new List<string>());
    }

    public static IPolicyAckEstimateStore NewCachedPolicyAckEstimateStore(MemoryPolicyAckEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckEstimateStore(inner, PolicyAckEstimateCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PolicyAckEstimate> FindPolicyAckEstimates(IPolicyAckEstimateStore store, PolicyAckEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
