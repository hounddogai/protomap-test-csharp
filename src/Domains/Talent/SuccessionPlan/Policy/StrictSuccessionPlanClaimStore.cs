namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict succession plan claims by code.</summary>
public interface IStrictSuccessionPlanClaimStore
{
    StrictSuccessionPlanClaim? Find(string code);
    void Save(StrictSuccessionPlanClaim item);
    bool Remove(string code);
    List<StrictSuccessionPlanClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictSuccessionPlanClaimStore(Dictionary<string, StrictSuccessionPlanClaim> items, List<string> order) : IStrictSuccessionPlanClaimStore
{
    public Dictionary<string, StrictSuccessionPlanClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictSuccessionPlanClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictSuccessionPlanClaim item)
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

    public List<StrictSuccessionPlanClaim> List()
    {
        var outValue = new List<StrictSuccessionPlanClaim>();
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
public sealed class CachedStrictSuccessionPlanClaimStore(MemoryStrictSuccessionPlanClaimStore inner, StrictSuccessionPlanClaimCache cache, int hits, int misses) : IStrictSuccessionPlanClaimStore
{
    public MemoryStrictSuccessionPlanClaimStore Inner { get; set; } = inner;
    public StrictSuccessionPlanClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictSuccessionPlanClaim? Find(string code)
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

    public void Save(StrictSuccessionPlanClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictSuccessionPlanClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict succession plan claims. Empty fields match all.</summary>
public sealed record StrictSuccessionPlanClaimQuery(string Label, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(StrictSuccessionPlanClaim item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<StrictSuccessionPlanClaim> Apply(List<StrictSuccessionPlanClaim> items)
    {
        var matched = Collections.Filter(items, (StrictSuccessionPlanClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict succession plan claim store.</summary>
public static class StrictSuccessionPlanClaimStore
{
    public static IStrictSuccessionPlanClaimStore NewMemoryStrictSuccessionPlanClaimStore()
    {
        return new MemoryStrictSuccessionPlanClaimStore(
            new Dictionary<string, StrictSuccessionPlanClaim>(),
            new List<string>());
    }

    public static IStrictSuccessionPlanClaimStore NewCachedStrictSuccessionPlanClaimStore(MemoryStrictSuccessionPlanClaimStore inner, int ttl, IClock clock)
    {
        return new CachedStrictSuccessionPlanClaimStore(
            inner,
            StrictSuccessionPlanClaimCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StrictSuccessionPlanClaim> FindStrictSuccessionPlanClaims(IStrictSuccessionPlanClaimStore store, StrictSuccessionPlanClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
