namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip claims by code.</summary>
public interface IPayslipClaimStore
{
    PayslipClaim? Find(string code);
    void Save(PayslipClaim item);
    bool Remove(string code);
    List<PayslipClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipClaimStore(Dictionary<string, PayslipClaim> items, List<string> order) : IPayslipClaimStore
{
    public Dictionary<string, PayslipClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipClaim item)
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

    public List<PayslipClaim> List()
    {
        var outValue = new List<PayslipClaim>();
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
public sealed class CachedPayslipClaimStore(MemoryPayslipClaimStore inner, PayslipClaimCache cache, int hits, int misses) : IPayslipClaimStore
{
    public MemoryPayslipClaimStore Inner { get; set; } = inner;
    public PayslipClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipClaim? Find(string code)
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

    public void Save(PayslipClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip claims. Empty fields match all.</summary>
public sealed record PayslipClaimQuery(string Category, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipClaim item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipClaim> Apply(List<PayslipClaim> items)
    {
        var matched = Collections.Filter(items, (PayslipClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip claim store.</summary>
public static class PayslipClaimStore
{
    public static IPayslipClaimStore NewMemoryPayslipClaimStore()
    {
        return new MemoryPayslipClaimStore(new Dictionary<string, PayslipClaim>(), new List<string>());
    }

    public static IPayslipClaimStore NewCachedPayslipClaimStore(MemoryPayslipClaimStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipClaimStore(inner, PayslipClaimCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayslipClaim> FindPayslipClaims(IPayslipClaimStore store, PayslipClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
