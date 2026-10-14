namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack scores by code.</summary>
public interface IPolicyAckScoreStore
{
    PolicyAckScore? Find(string code);
    void Save(PolicyAckScore item);
    bool Remove(string code);
    List<PolicyAckScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckScoreStore(Dictionary<string, PolicyAckScore> items, List<string> order) : IPolicyAckScoreStore
{
    public Dictionary<string, PolicyAckScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckScore item)
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

    public List<PolicyAckScore> List()
    {
        var outValue = new List<PolicyAckScore>();
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
public sealed class CachedPolicyAckScoreStore(MemoryPolicyAckScoreStore inner, PolicyAckScoreCache cache, int hits, int misses) : IPolicyAckScoreStore
{
    public MemoryPolicyAckScoreStore Inner { get; set; } = inner;
    public PolicyAckScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckScore? Find(string code)
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

    public void Save(PolicyAckScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack scores. Empty fields match all.</summary>
public sealed record PolicyAckScoreQuery(string Country, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckScore item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckScore> Apply(List<PolicyAckScore> items)
    {
        var matched = Collections.Filter(items, (PolicyAckScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack score store.</summary>
public static class PolicyAckScoreStore
{
    public static IPolicyAckScoreStore NewMemoryPolicyAckScoreStore()
    {
        return new MemoryPolicyAckScoreStore(new Dictionary<string, PolicyAckScore>(), new List<string>());
    }

    public static IPolicyAckScoreStore NewCachedPolicyAckScoreStore(MemoryPolicyAckScoreStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckScoreStore(inner, PolicyAckScoreCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PolicyAckScore> FindPolicyAckScores(IPolicyAckScoreStore store, PolicyAckScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
