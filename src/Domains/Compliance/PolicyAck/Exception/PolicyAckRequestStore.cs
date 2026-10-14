namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack requests by code.</summary>
public interface IPolicyAckRequestStore
{
    PolicyAckRequest? Find(string code);
    void Save(PolicyAckRequest item);
    bool Remove(string code);
    List<PolicyAckRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckRequestStore(Dictionary<string, PolicyAckRequest> items, List<string> order) : IPolicyAckRequestStore
{
    public Dictionary<string, PolicyAckRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckRequest item)
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

    public List<PolicyAckRequest> List()
    {
        var outValue = new List<PolicyAckRequest>();
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
public sealed class CachedPolicyAckRequestStore(MemoryPolicyAckRequestStore inner, PolicyAckRequestCache cache, int hits, int misses) : IPolicyAckRequestStore
{
    public MemoryPolicyAckRequestStore Inner { get; set; } = inner;
    public PolicyAckRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckRequest? Find(string code)
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

    public void Save(PolicyAckRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack requests. Empty fields match all.</summary>
public sealed record PolicyAckRequestQuery(string Segment, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckRequest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckRequest> Apply(List<PolicyAckRequest> items)
    {
        var matched = Collections.Filter(items, (PolicyAckRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack request store.</summary>
public static class PolicyAckRequestStore
{
    public static IPolicyAckRequestStore NewMemoryPolicyAckRequestStore()
    {
        return new MemoryPolicyAckRequestStore(new Dictionary<string, PolicyAckRequest>(), new List<string>());
    }

    public static IPolicyAckRequestStore NewCachedPolicyAckRequestStore(MemoryPolicyAckRequestStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckRequestStore(inner, PolicyAckRequestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PolicyAckRequest> FindPolicyAckRequests(IPolicyAckRequestStore store, PolicyAckRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
