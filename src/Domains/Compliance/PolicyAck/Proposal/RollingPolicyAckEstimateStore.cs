namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling policy ack estimates by code.</summary>
public interface IRollingPolicyAckEstimateStore
{
    RollingPolicyAckEstimate? Find(string code);
    void Save(RollingPolicyAckEstimate item);
    bool Remove(string code);
    List<RollingPolicyAckEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingPolicyAckEstimateStore(Dictionary<string, RollingPolicyAckEstimate> items, List<string> order) : IRollingPolicyAckEstimateStore
{
    public Dictionary<string, RollingPolicyAckEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingPolicyAckEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingPolicyAckEstimate item)
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

    public List<RollingPolicyAckEstimate> List()
    {
        var outValue = new List<RollingPolicyAckEstimate>();
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
public sealed class CachedRollingPolicyAckEstimateStore(MemoryRollingPolicyAckEstimateStore inner, RollingPolicyAckEstimateCache cache, int hits, int misses) : IRollingPolicyAckEstimateStore
{
    public MemoryRollingPolicyAckEstimateStore Inner { get; set; } = inner;
    public RollingPolicyAckEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingPolicyAckEstimate? Find(string code)
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

    public void Save(RollingPolicyAckEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingPolicyAckEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling policy ack estimates. Empty fields match all.</summary>
public sealed record RollingPolicyAckEstimateQuery(string Title, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(RollingPolicyAckEstimate item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingPolicyAckEstimate> Apply(List<RollingPolicyAckEstimate> items)
    {
        var matched = Collections.Filter(items, (RollingPolicyAckEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling policy ack estimate store.</summary>
public static class RollingPolicyAckEstimateStore
{
    public static IRollingPolicyAckEstimateStore NewMemoryRollingPolicyAckEstimateStore()
    {
        return new MemoryRollingPolicyAckEstimateStore(
            new Dictionary<string, RollingPolicyAckEstimate>(),
            new List<string>());
    }

    public static IRollingPolicyAckEstimateStore NewCachedRollingPolicyAckEstimateStore(MemoryRollingPolicyAckEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedRollingPolicyAckEstimateStore(
            inner,
            RollingPolicyAckEstimateCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RollingPolicyAckEstimate> FindRollingPolicyAckEstimates(IRollingPolicyAckEstimateStore store, RollingPolicyAckEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
