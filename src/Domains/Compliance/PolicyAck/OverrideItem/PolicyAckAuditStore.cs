namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack audits by code.</summary>
public interface IPolicyAckAuditStore
{
    PolicyAckAudit? Find(string code);
    void Save(PolicyAckAudit item);
    bool Remove(string code);
    List<PolicyAckAudit> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckAuditStore(Dictionary<string, PolicyAckAudit> items, List<string> order) : IPolicyAckAuditStore
{
    public Dictionary<string, PolicyAckAudit> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckAudit? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckAudit item)
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

    public List<PolicyAckAudit> List()
    {
        var outValue = new List<PolicyAckAudit>();
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
public sealed class CachedPolicyAckAuditStore(MemoryPolicyAckAuditStore inner, PolicyAckAuditCache cache, int hits, int misses) : IPolicyAckAuditStore
{
    public MemoryPolicyAckAuditStore Inner { get; set; } = inner;
    public PolicyAckAuditCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckAudit? Find(string code)
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

    public void Save(PolicyAckAudit item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckAudit> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack audits. Empty fields match all.</summary>
public sealed record PolicyAckAuditQuery(string Category, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckAudit item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckAudit> Apply(List<PolicyAckAudit> items)
    {
        var matched = Collections.Filter(items, (PolicyAckAudit candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack audit store.</summary>
public static class PolicyAckAuditStore
{
    public static IPolicyAckAuditStore NewMemoryPolicyAckAuditStore()
    {
        return new MemoryPolicyAckAuditStore(new Dictionary<string, PolicyAckAudit>(), new List<string>());
    }

    public static IPolicyAckAuditStore NewCachedPolicyAckAuditStore(MemoryPolicyAckAuditStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckAuditStore(inner, PolicyAckAuditCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PolicyAckAudit> FindPolicyAckAudits(IPolicyAckAuditStore store, PolicyAckAuditQuery query)
    {
        return query.Apply(store.List());
    }
}
