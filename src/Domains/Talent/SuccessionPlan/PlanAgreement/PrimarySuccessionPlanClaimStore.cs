namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary succession plan claims by code.</summary>
public interface IPrimarySuccessionPlanClaimStore
{
    PrimarySuccessionPlanClaim? Find(string code);
    void Save(PrimarySuccessionPlanClaim item);
    bool Remove(string code);
    List<PrimarySuccessionPlanClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimarySuccessionPlanClaimStore(Dictionary<string, PrimarySuccessionPlanClaim> items, List<string> order) : IPrimarySuccessionPlanClaimStore
{
    public Dictionary<string, PrimarySuccessionPlanClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimarySuccessionPlanClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimarySuccessionPlanClaim item)
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

    public List<PrimarySuccessionPlanClaim> List()
    {
        var outValue = new List<PrimarySuccessionPlanClaim>();
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
public sealed class CachedPrimarySuccessionPlanClaimStore(MemoryPrimarySuccessionPlanClaimStore inner, PrimarySuccessionPlanClaimCache cache, int hits, int misses) : IPrimarySuccessionPlanClaimStore
{
    public MemoryPrimarySuccessionPlanClaimStore Inner { get; set; } = inner;
    public PrimarySuccessionPlanClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimarySuccessionPlanClaim? Find(string code)
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

    public void Save(PrimarySuccessionPlanClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimarySuccessionPlanClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary succession plan claims. Empty fields match all.</summary>
public sealed record PrimarySuccessionPlanClaimQuery(string Locale, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(PrimarySuccessionPlanClaim item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimarySuccessionPlanClaim> Apply(List<PrimarySuccessionPlanClaim> items)
    {
        var matched = Collections.Filter(items, (PrimarySuccessionPlanClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary succession plan claim store.</summary>
public static class PrimarySuccessionPlanClaimStore
{
    public static IPrimarySuccessionPlanClaimStore NewMemoryPrimarySuccessionPlanClaimStore()
    {
        return new MemoryPrimarySuccessionPlanClaimStore(
            new Dictionary<string, PrimarySuccessionPlanClaim>(),
            new List<string>());
    }

    public static IPrimarySuccessionPlanClaimStore NewCachedPrimarySuccessionPlanClaimStore(MemoryPrimarySuccessionPlanClaimStore inner, int ttl, IClock clock)
    {
        return new CachedPrimarySuccessionPlanClaimStore(
            inner,
            PrimarySuccessionPlanClaimCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimarySuccessionPlanClaim> FindPrimarySuccessionPlanClaims(IPrimarySuccessionPlanClaimStore store, PrimarySuccessionPlanClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
