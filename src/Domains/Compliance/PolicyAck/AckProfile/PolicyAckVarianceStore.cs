namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack variances by code.</summary>
public interface IPolicyAckVarianceStore
{
    PolicyAckVariance? Find(string code);
    void Save(PolicyAckVariance item);
    bool Remove(string code);
    List<PolicyAckVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckVarianceStore(Dictionary<string, PolicyAckVariance> items, List<string> order) : IPolicyAckVarianceStore
{
    public Dictionary<string, PolicyAckVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckVariance item)
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

    public List<PolicyAckVariance> List()
    {
        var outValue = new List<PolicyAckVariance>();
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
public sealed class CachedPolicyAckVarianceStore(MemoryPolicyAckVarianceStore inner, PolicyAckVarianceCache cache, int hits, int misses) : IPolicyAckVarianceStore
{
    public MemoryPolicyAckVarianceStore Inner { get; set; } = inner;
    public PolicyAckVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckVariance? Find(string code)
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

    public void Save(PolicyAckVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack variances. Empty fields match all.</summary>
public sealed record PolicyAckVarianceQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckVariance item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckVariance> Apply(List<PolicyAckVariance> items)
    {
        var matched = Collections.Filter(items, (PolicyAckVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack variance store.</summary>
public static class PolicyAckVarianceStore
{
    public static IPolicyAckVarianceStore NewMemoryPolicyAckVarianceStore()
    {
        return new MemoryPolicyAckVarianceStore(new Dictionary<string, PolicyAckVariance>(), new List<string>());
    }

    public static IPolicyAckVarianceStore NewCachedPolicyAckVarianceStore(MemoryPolicyAckVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckVarianceStore(inner, PolicyAckVarianceCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PolicyAckVariance> FindPolicyAckVariances(IPolicyAckVarianceStore store, PolicyAckVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
