namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack adjustments by code.</summary>
public interface IPolicyAckAdjustmentStore
{
    PolicyAckAdjustment? Find(string code);
    void Save(PolicyAckAdjustment item);
    bool Remove(string code);
    List<PolicyAckAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckAdjustmentStore(Dictionary<string, PolicyAckAdjustment> items, List<string> order) : IPolicyAckAdjustmentStore
{
    public Dictionary<string, PolicyAckAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckAdjustment item)
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

    public List<PolicyAckAdjustment> List()
    {
        var outValue = new List<PolicyAckAdjustment>();
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
public sealed class CachedPolicyAckAdjustmentStore(MemoryPolicyAckAdjustmentStore inner, PolicyAckAdjustmentCache cache, int hits, int misses) : IPolicyAckAdjustmentStore
{
    public MemoryPolicyAckAdjustmentStore Inner { get; set; } = inner;
    public PolicyAckAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckAdjustment? Find(string code)
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

    public void Save(PolicyAckAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack adjustments. Empty fields match all.</summary>
public sealed record PolicyAckAdjustmentQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckAdjustment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckAdjustment> Apply(List<PolicyAckAdjustment> items)
    {
        var matched = Collections.Filter(items, (PolicyAckAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack adjustment store.</summary>
public static class PolicyAckAdjustmentStore
{
    public static IPolicyAckAdjustmentStore NewMemoryPolicyAckAdjustmentStore()
    {
        return new MemoryPolicyAckAdjustmentStore(new Dictionary<string, PolicyAckAdjustment>(), new List<string>());
    }

    public static IPolicyAckAdjustmentStore NewCachedPolicyAckAdjustmentStore(MemoryPolicyAckAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckAdjustmentStore(inner, PolicyAckAdjustmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PolicyAckAdjustment> FindPolicyAckAdjustments(IPolicyAckAdjustmentStore store, PolicyAckAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
