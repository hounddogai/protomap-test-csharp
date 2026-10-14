namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient succession plan claims by code.</summary>
public interface ILenientSuccessionPlanClaimStore
{
    LenientSuccessionPlanClaim? Find(string code);
    void Save(LenientSuccessionPlanClaim item);
    bool Remove(string code);
    List<LenientSuccessionPlanClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientSuccessionPlanClaimStore(Dictionary<string, LenientSuccessionPlanClaim> items, List<string> order) : ILenientSuccessionPlanClaimStore
{
    public Dictionary<string, LenientSuccessionPlanClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientSuccessionPlanClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientSuccessionPlanClaim item)
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

    public List<LenientSuccessionPlanClaim> List()
    {
        var outValue = new List<LenientSuccessionPlanClaim>();
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
public sealed class CachedLenientSuccessionPlanClaimStore(MemoryLenientSuccessionPlanClaimStore inner, LenientSuccessionPlanClaimCache cache, int hits, int misses) : ILenientSuccessionPlanClaimStore
{
    public MemoryLenientSuccessionPlanClaimStore Inner { get; set; } = inner;
    public LenientSuccessionPlanClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientSuccessionPlanClaim? Find(string code)
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

    public void Save(LenientSuccessionPlanClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientSuccessionPlanClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient succession plan claims. Empty fields match all.</summary>
public sealed record LenientSuccessionPlanClaimQuery(string Source, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(LenientSuccessionPlanClaim item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientSuccessionPlanClaim> Apply(List<LenientSuccessionPlanClaim> items)
    {
        var matched = Collections.Filter(items, (LenientSuccessionPlanClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient succession plan claim store.</summary>
public static class LenientSuccessionPlanClaimStore
{
    public static ILenientSuccessionPlanClaimStore NewMemoryLenientSuccessionPlanClaimStore()
    {
        return new MemoryLenientSuccessionPlanClaimStore(
            new Dictionary<string, LenientSuccessionPlanClaim>(),
            new List<string>());
    }

    public static ILenientSuccessionPlanClaimStore NewCachedLenientSuccessionPlanClaimStore(MemoryLenientSuccessionPlanClaimStore inner, int ttl, IClock clock)
    {
        return new CachedLenientSuccessionPlanClaimStore(
            inner,
            LenientSuccessionPlanClaimCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientSuccessionPlanClaim> FindLenientSuccessionPlanClaims(ILenientSuccessionPlanClaimStore store, LenientSuccessionPlanClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
