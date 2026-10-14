namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack digests by code.</summary>
public interface IPolicyAckDigestStore
{
    PolicyAckDigest? Find(string code);
    void Save(PolicyAckDigest item);
    bool Remove(string code);
    List<PolicyAckDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckDigestStore(Dictionary<string, PolicyAckDigest> items, List<string> order) : IPolicyAckDigestStore
{
    public Dictionary<string, PolicyAckDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckDigest item)
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

    public List<PolicyAckDigest> List()
    {
        var outValue = new List<PolicyAckDigest>();
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
public sealed class CachedPolicyAckDigestStore(MemoryPolicyAckDigestStore inner, PolicyAckDigestCache cache, int hits, int misses) : IPolicyAckDigestStore
{
    public MemoryPolicyAckDigestStore Inner { get; set; } = inner;
    public PolicyAckDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckDigest? Find(string code)
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

    public void Save(PolicyAckDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack digests. Empty fields match all.</summary>
public sealed record PolicyAckDigestQuery(string Title, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckDigest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckDigest> Apply(List<PolicyAckDigest> items)
    {
        var matched = Collections.Filter(items, (PolicyAckDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack digest store.</summary>
public static class PolicyAckDigestStore
{
    public static IPolicyAckDigestStore NewMemoryPolicyAckDigestStore()
    {
        return new MemoryPolicyAckDigestStore(new Dictionary<string, PolicyAckDigest>(), new List<string>());
    }

    public static IPolicyAckDigestStore NewCachedPolicyAckDigestStore(MemoryPolicyAckDigestStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckDigestStore(inner, PolicyAckDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PolicyAckDigest> FindPolicyAckDigests(IPolicyAckDigestStore store, PolicyAckDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
