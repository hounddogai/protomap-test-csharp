namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack baselines by code.</summary>
public interface IPolicyAckBaselineStore
{
    PolicyAckBaseline? Find(string code);
    void Save(PolicyAckBaseline item);
    bool Remove(string code);
    List<PolicyAckBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckBaselineStore(Dictionary<string, PolicyAckBaseline> items, List<string> order) : IPolicyAckBaselineStore
{
    public Dictionary<string, PolicyAckBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckBaseline item)
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

    public List<PolicyAckBaseline> List()
    {
        var outValue = new List<PolicyAckBaseline>();
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
public sealed class CachedPolicyAckBaselineStore(MemoryPolicyAckBaselineStore inner, PolicyAckBaselineCache cache, int hits, int misses) : IPolicyAckBaselineStore
{
    public MemoryPolicyAckBaselineStore Inner { get; set; } = inner;
    public PolicyAckBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckBaseline? Find(string code)
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

    public void Save(PolicyAckBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack baselines. Empty fields match all.</summary>
public sealed record PolicyAckBaselineQuery(string Segment, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckBaseline item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<PolicyAckBaseline> Apply(List<PolicyAckBaseline> items)
    {
        var matched = Collections.Filter(items, (PolicyAckBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack baseline store.</summary>
public static class PolicyAckBaselineStore
{
    public static IPolicyAckBaselineStore NewMemoryPolicyAckBaselineStore()
    {
        return new MemoryPolicyAckBaselineStore(new Dictionary<string, PolicyAckBaseline>(), new List<string>());
    }

    public static IPolicyAckBaselineStore NewCachedPolicyAckBaselineStore(MemoryPolicyAckBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckBaselineStore(inner, PolicyAckBaselineCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PolicyAckBaseline> FindPolicyAckBaselines(IPolicyAckBaselineStore store, PolicyAckBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
