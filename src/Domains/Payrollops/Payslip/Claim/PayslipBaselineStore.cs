namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip baselines by code.</summary>
public interface IPayslipBaselineStore
{
    PayslipBaseline? Find(string code);
    void Save(PayslipBaseline item);
    bool Remove(string code);
    List<PayslipBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipBaselineStore(Dictionary<string, PayslipBaseline> items, List<string> order) : IPayslipBaselineStore
{
    public Dictionary<string, PayslipBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipBaseline item)
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

    public List<PayslipBaseline> List()
    {
        var outValue = new List<PayslipBaseline>();
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
public sealed class CachedPayslipBaselineStore(MemoryPayslipBaselineStore inner, PayslipBaselineCache cache, int hits, int misses) : IPayslipBaselineStore
{
    public MemoryPayslipBaselineStore Inner { get; set; } = inner;
    public PayslipBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipBaseline? Find(string code)
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

    public void Save(PayslipBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip baselines. Empty fields match all.</summary>
public sealed record PayslipBaselineQuery(string Locale, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipBaseline item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipBaseline> Apply(List<PayslipBaseline> items)
    {
        var matched = Collections.Filter(items, (PayslipBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip baseline store.</summary>
public static class PayslipBaselineStore
{
    public static IPayslipBaselineStore NewMemoryPayslipBaselineStore()
    {
        return new MemoryPayslipBaselineStore(new Dictionary<string, PayslipBaseline>(), new List<string>());
    }

    public static IPayslipBaselineStore NewCachedPayslipBaselineStore(MemoryPayslipBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipBaselineStore(inner, PayslipBaselineCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayslipBaseline> FindPayslipBaselines(IPayslipBaselineStore store, PayslipBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
