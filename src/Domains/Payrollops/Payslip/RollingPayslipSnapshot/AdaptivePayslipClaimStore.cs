namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive payslip claims by code.</summary>
public interface IAdaptivePayslipClaimStore
{
    AdaptivePayslipClaim? Find(string code);
    void Save(AdaptivePayslipClaim item);
    bool Remove(string code);
    List<AdaptivePayslipClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptivePayslipClaimStore(Dictionary<string, AdaptivePayslipClaim> items, List<string> order) : IAdaptivePayslipClaimStore
{
    public Dictionary<string, AdaptivePayslipClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptivePayslipClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptivePayslipClaim item)
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

    public List<AdaptivePayslipClaim> List()
    {
        var outValue = new List<AdaptivePayslipClaim>();
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
public sealed class CachedAdaptivePayslipClaimStore(MemoryAdaptivePayslipClaimStore inner, AdaptivePayslipClaimCache cache, int hits, int misses) : IAdaptivePayslipClaimStore
{
    public MemoryAdaptivePayslipClaimStore Inner { get; set; } = inner;
    public AdaptivePayslipClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptivePayslipClaim? Find(string code)
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

    public void Save(AdaptivePayslipClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptivePayslipClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive payslip claims. Empty fields match all.</summary>
public sealed record AdaptivePayslipClaimQuery(string Team, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptivePayslipClaim item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptivePayslipClaim> Apply(List<AdaptivePayslipClaim> items)
    {
        var matched = Collections.Filter(items, (AdaptivePayslipClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive payslip claim store.</summary>
public static class AdaptivePayslipClaimStore
{
    public static IAdaptivePayslipClaimStore NewMemoryAdaptivePayslipClaimStore()
    {
        return new MemoryAdaptivePayslipClaimStore(
            new Dictionary<string, AdaptivePayslipClaim>(),
            new List<string>());
    }

    public static IAdaptivePayslipClaimStore NewCachedAdaptivePayslipClaimStore(MemoryAdaptivePayslipClaimStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptivePayslipClaimStore(inner, AdaptivePayslipClaimCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AdaptivePayslipClaim> FindAdaptivePayslipClaims(IAdaptivePayslipClaimStore store, AdaptivePayslipClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
