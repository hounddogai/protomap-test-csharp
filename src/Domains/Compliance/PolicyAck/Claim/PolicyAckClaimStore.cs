namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack claims by code.</summary>
public interface IPolicyAckClaimStore
{
    PolicyAckClaim? Find(string code);
    void Save(PolicyAckClaim item);
    bool Remove(string code);
    List<PolicyAckClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckClaimStore(Dictionary<string, PolicyAckClaim> items, List<string> order) : IPolicyAckClaimStore
{
    public Dictionary<string, PolicyAckClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckClaim item)
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

    public List<PolicyAckClaim> List()
    {
        var outValue = new List<PolicyAckClaim>();
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
public sealed class CachedPolicyAckClaimStore(MemoryPolicyAckClaimStore inner, PolicyAckClaimCache cache, int hits, int misses) : IPolicyAckClaimStore
{
    public MemoryPolicyAckClaimStore Inner { get; set; } = inner;
    public PolicyAckClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckClaim? Find(string code)
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

    public void Save(PolicyAckClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack claims. Empty fields match all.</summary>
public sealed record PolicyAckClaimQuery(string Category, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckClaim item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckClaim> Apply(List<PolicyAckClaim> items)
    {
        var matched = Collections.Filter(items, (PolicyAckClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack claim store.</summary>
public static class PolicyAckClaimStore
{
    public static IPolicyAckClaimStore NewMemoryPolicyAckClaimStore()
    {
        return new MemoryPolicyAckClaimStore(new Dictionary<string, PolicyAckClaim>(), new List<string>());
    }

    public static IPolicyAckClaimStore NewCachedPolicyAckClaimStore(MemoryPolicyAckClaimStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckClaimStore(inner, PolicyAckClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PolicyAckClaim> FindPolicyAckClaims(IPolicyAckClaimStore store, PolicyAckClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
