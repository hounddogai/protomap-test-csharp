namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy policy ack claims by code.</summary>
public interface ILegacyPolicyAckClaimStore
{
    LegacyPolicyAckClaim? Find(string code);
    void Save(LegacyPolicyAckClaim item);
    bool Remove(string code);
    List<LegacyPolicyAckClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPolicyAckClaimStore(Dictionary<string, LegacyPolicyAckClaim> items, List<string> order) : ILegacyPolicyAckClaimStore
{
    public Dictionary<string, LegacyPolicyAckClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPolicyAckClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPolicyAckClaim item)
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

    public List<LegacyPolicyAckClaim> List()
    {
        var outValue = new List<LegacyPolicyAckClaim>();
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
public sealed class CachedLegacyPolicyAckClaimStore(MemoryLegacyPolicyAckClaimStore inner, LegacyPolicyAckClaimCache cache, int hits, int misses) : ILegacyPolicyAckClaimStore
{
    public MemoryLegacyPolicyAckClaimStore Inner { get; set; } = inner;
    public LegacyPolicyAckClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPolicyAckClaim? Find(string code)
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

    public void Save(LegacyPolicyAckClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPolicyAckClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy policy ack claims. Empty fields match all.</summary>
public sealed record LegacyPolicyAckClaimQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPolicyAckClaim item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyPolicyAckClaim> Apply(List<LegacyPolicyAckClaim> items)
    {
        var matched = Collections.Filter(items, (LegacyPolicyAckClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy policy ack claim store.</summary>
public static class LegacyPolicyAckClaimStore
{
    public static ILegacyPolicyAckClaimStore NewMemoryLegacyPolicyAckClaimStore()
    {
        return new MemoryLegacyPolicyAckClaimStore(
            new Dictionary<string, LegacyPolicyAckClaim>(),
            new List<string>());
    }

    public static ILegacyPolicyAckClaimStore NewCachedLegacyPolicyAckClaimStore(MemoryLegacyPolicyAckClaimStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPolicyAckClaimStore(inner, LegacyPolicyAckClaimCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LegacyPolicyAckClaim> FindLegacyPolicyAckClaims(ILegacyPolicyAckClaimStore store, LegacyPolicyAckClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
