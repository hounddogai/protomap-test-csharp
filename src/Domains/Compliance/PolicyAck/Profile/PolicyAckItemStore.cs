namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack items by code.</summary>
public interface IPolicyAckItemStore
{
    PolicyAckItem? Find(string code);
    void Save(PolicyAckItem item);
    bool Remove(string code);
    List<PolicyAckItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckItemStore(Dictionary<string, PolicyAckItem> items, List<string> order) : IPolicyAckItemStore
{
    public Dictionary<string, PolicyAckItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckItem item)
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

    public List<PolicyAckItem> List()
    {
        var outValue = new List<PolicyAckItem>();
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
public sealed class CachedPolicyAckItemStore(MemoryPolicyAckItemStore inner, PolicyAckItemCache cache, int hits, int misses) : IPolicyAckItemStore
{
    public MemoryPolicyAckItemStore Inner { get; set; } = inner;
    public PolicyAckItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckItem? Find(string code)
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

    public void Save(PolicyAckItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack items. Empty fields match all.</summary>
public sealed record PolicyAckItemQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckItem item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckItem> Apply(List<PolicyAckItem> items)
    {
        var matched = Collections.Filter(items, (PolicyAckItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack item store.</summary>
public static class PolicyAckItemStore
{
    public static IPolicyAckItemStore NewMemoryPolicyAckItemStore()
    {
        return new MemoryPolicyAckItemStore(new Dictionary<string, PolicyAckItem>(), new List<string>());
    }

    public static IPolicyAckItemStore NewCachedPolicyAckItemStore(MemoryPolicyAckItemStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckItemStore(inner, PolicyAckItemCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PolicyAckItem> FindPolicyAckItems(IPolicyAckItemStore store, PolicyAckItemQuery query)
    {
        return query.Apply(store.List());
    }
}
