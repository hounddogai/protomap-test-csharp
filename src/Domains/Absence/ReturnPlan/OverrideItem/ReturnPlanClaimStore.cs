namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan claims by code.</summary>
public interface IReturnPlanClaimStore
{
    ReturnPlanClaim? Find(string code);
    void Save(ReturnPlanClaim item);
    bool Remove(string code);
    List<ReturnPlanClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanClaimStore(Dictionary<string, ReturnPlanClaim> items, List<string> order) : IReturnPlanClaimStore
{
    public Dictionary<string, ReturnPlanClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanClaim item)
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

    public List<ReturnPlanClaim> List()
    {
        var outValue = new List<ReturnPlanClaim>();
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
public sealed class CachedReturnPlanClaimStore(MemoryReturnPlanClaimStore inner, ReturnPlanClaimCache cache, int hits, int misses) : IReturnPlanClaimStore
{
    public MemoryReturnPlanClaimStore Inner { get; set; } = inner;
    public ReturnPlanClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanClaim? Find(string code)
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

    public void Save(ReturnPlanClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan claims. Empty fields match all.</summary>
public sealed record ReturnPlanClaimQuery(string Segment, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanClaim item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanClaim> Apply(List<ReturnPlanClaim> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan claim store.</summary>
public static class ReturnPlanClaimStore
{
    public static IReturnPlanClaimStore NewMemoryReturnPlanClaimStore()
    {
        return new MemoryReturnPlanClaimStore(new Dictionary<string, ReturnPlanClaim>(), new List<string>());
    }

    public static IReturnPlanClaimStore NewCachedReturnPlanClaimStore(MemoryReturnPlanClaimStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanClaimStore(inner, ReturnPlanClaimCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanClaim> FindReturnPlanClaims(IReturnPlanClaimStore store, ReturnPlanClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
