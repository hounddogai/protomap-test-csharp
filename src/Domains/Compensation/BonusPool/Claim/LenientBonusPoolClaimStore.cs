namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient bonus pool claims by code.</summary>
public interface ILenientBonusPoolClaimStore
{
    LenientBonusPoolClaim? Find(string code);
    void Save(LenientBonusPoolClaim item);
    bool Remove(string code);
    List<LenientBonusPoolClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientBonusPoolClaimStore(Dictionary<string, LenientBonusPoolClaim> items, List<string> order) : ILenientBonusPoolClaimStore
{
    public Dictionary<string, LenientBonusPoolClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientBonusPoolClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientBonusPoolClaim item)
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

    public List<LenientBonusPoolClaim> List()
    {
        var outValue = new List<LenientBonusPoolClaim>();
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
public sealed class CachedLenientBonusPoolClaimStore(MemoryLenientBonusPoolClaimStore inner, LenientBonusPoolClaimCache cache, int hits, int misses) : ILenientBonusPoolClaimStore
{
    public MemoryLenientBonusPoolClaimStore Inner { get; set; } = inner;
    public LenientBonusPoolClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientBonusPoolClaim? Find(string code)
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

    public void Save(LenientBonusPoolClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientBonusPoolClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient bonus pool claims. Empty fields match all.</summary>
public sealed record LenientBonusPoolClaimQuery(string Segment, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(LenientBonusPoolClaim item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientBonusPoolClaim> Apply(List<LenientBonusPoolClaim> items)
    {
        var matched = Collections.Filter(items, (LenientBonusPoolClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient bonus pool claim store.</summary>
public static class LenientBonusPoolClaimStore
{
    public static ILenientBonusPoolClaimStore NewMemoryLenientBonusPoolClaimStore()
    {
        return new MemoryLenientBonusPoolClaimStore(
            new Dictionary<string, LenientBonusPoolClaim>(),
            new List<string>());
    }

    public static ILenientBonusPoolClaimStore NewCachedLenientBonusPoolClaimStore(MemoryLenientBonusPoolClaimStore inner, int ttl, IClock clock)
    {
        return new CachedLenientBonusPoolClaimStore(
            inner,
            LenientBonusPoolClaimCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientBonusPoolClaim> FindLenientBonusPoolClaims(ILenientBonusPoolClaimStore store, LenientBonusPoolClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
