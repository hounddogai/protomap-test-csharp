namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict policy ack claims by code.</summary>
public interface IStrictPolicyAckClaimStore
{
    StrictPolicyAckClaim? Find(string code);
    void Save(StrictPolicyAckClaim item);
    bool Remove(string code);
    List<StrictPolicyAckClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPolicyAckClaimStore(Dictionary<string, StrictPolicyAckClaim> items, List<string> order) : IStrictPolicyAckClaimStore
{
    public Dictionary<string, StrictPolicyAckClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPolicyAckClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPolicyAckClaim item)
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

    public List<StrictPolicyAckClaim> List()
    {
        var outValue = new List<StrictPolicyAckClaim>();
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
public sealed class CachedStrictPolicyAckClaimStore(MemoryStrictPolicyAckClaimStore inner, StrictPolicyAckClaimCache cache, int hits, int misses) : IStrictPolicyAckClaimStore
{
    public MemoryStrictPolicyAckClaimStore Inner { get; set; } = inner;
    public StrictPolicyAckClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPolicyAckClaim? Find(string code)
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

    public void Save(StrictPolicyAckClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPolicyAckClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict policy ack claims. Empty fields match all.</summary>
public sealed record StrictPolicyAckClaimQuery(string Channel, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPolicyAckClaim item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<StrictPolicyAckClaim> Apply(List<StrictPolicyAckClaim> items)
    {
        var matched = Collections.Filter(items, (StrictPolicyAckClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict policy ack claim store.</summary>
public static class StrictPolicyAckClaimStore
{
    public static IStrictPolicyAckClaimStore NewMemoryStrictPolicyAckClaimStore()
    {
        return new MemoryStrictPolicyAckClaimStore(
            new Dictionary<string, StrictPolicyAckClaim>(),
            new List<string>());
    }

    public static IStrictPolicyAckClaimStore NewCachedStrictPolicyAckClaimStore(MemoryStrictPolicyAckClaimStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPolicyAckClaimStore(inner, StrictPolicyAckClaimCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictPolicyAckClaim> FindStrictPolicyAckClaims(IStrictPolicyAckClaimStore store, StrictPolicyAckClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
