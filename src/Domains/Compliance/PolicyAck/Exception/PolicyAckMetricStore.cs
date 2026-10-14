namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack metrics by code.</summary>
public interface IPolicyAckMetricStore
{
    PolicyAckMetric? Find(string code);
    void Save(PolicyAckMetric item);
    bool Remove(string code);
    List<PolicyAckMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckMetricStore(Dictionary<string, PolicyAckMetric> items, List<string> order) : IPolicyAckMetricStore
{
    public Dictionary<string, PolicyAckMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckMetric item)
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

    public List<PolicyAckMetric> List()
    {
        var outValue = new List<PolicyAckMetric>();
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
public sealed class CachedPolicyAckMetricStore(MemoryPolicyAckMetricStore inner, PolicyAckMetricCache cache, int hits, int misses) : IPolicyAckMetricStore
{
    public MemoryPolicyAckMetricStore Inner { get; set; } = inner;
    public PolicyAckMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckMetric? Find(string code)
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

    public void Save(PolicyAckMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack metrics. Empty fields match all.</summary>
public sealed record PolicyAckMetricQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PolicyAckMetric> Apply(List<PolicyAckMetric> items)
    {
        var matched = Collections.Filter(items, (PolicyAckMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack metric store.</summary>
public static class PolicyAckMetricStore
{
    public static IPolicyAckMetricStore NewMemoryPolicyAckMetricStore()
    {
        return new MemoryPolicyAckMetricStore(new Dictionary<string, PolicyAckMetric>(), new List<string>());
    }

    public static IPolicyAckMetricStore NewCachedPolicyAckMetricStore(MemoryPolicyAckMetricStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckMetricStore(inner, PolicyAckMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PolicyAckMetric> FindPolicyAckMetrics(IPolicyAckMetricStore store, PolicyAckMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
