namespace Acme.Workforce.Compliance.PolicyAck.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack groups by code.</summary>
public interface IPolicyAckGroupStore
{
    PolicyAckGroup? Find(string code);
    void Save(PolicyAckGroup item);
    bool Remove(string code);
    List<PolicyAckGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckGroupStore(Dictionary<string, PolicyAckGroup> items, List<string> order) : IPolicyAckGroupStore
{
    public Dictionary<string, PolicyAckGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckGroup item)
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

    public List<PolicyAckGroup> List()
    {
        var outValue = new List<PolicyAckGroup>();
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
public sealed class CachedPolicyAckGroupStore(MemoryPolicyAckGroupStore inner, PolicyAckGroupCache cache, int hits, int misses) : IPolicyAckGroupStore
{
    public MemoryPolicyAckGroupStore Inner { get; set; } = inner;
    public PolicyAckGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckGroup? Find(string code)
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

    public void Save(PolicyAckGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack groups. Empty fields match all.</summary>
public sealed record PolicyAckGroupQuery(string Segment, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckGroup item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckGroup> Apply(List<PolicyAckGroup> items)
    {
        var matched = Collections.Filter(items, (PolicyAckGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack group store.</summary>
public static class PolicyAckGroupStore
{
    public static IPolicyAckGroupStore NewMemoryPolicyAckGroupStore()
    {
        return new MemoryPolicyAckGroupStore(new Dictionary<string, PolicyAckGroup>(), new List<string>());
    }

    public static IPolicyAckGroupStore NewCachedPolicyAckGroupStore(MemoryPolicyAckGroupStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckGroupStore(inner, PolicyAckGroupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PolicyAckGroup> FindPolicyAckGroups(IPolicyAckGroupStore store, PolicyAckGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
