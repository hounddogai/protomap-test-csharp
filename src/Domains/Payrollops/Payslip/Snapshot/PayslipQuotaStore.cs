namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip quotas by code.</summary>
public interface IPayslipQuotaStore
{
    PayslipQuota? Find(string code);
    void Save(PayslipQuota item);
    bool Remove(string code);
    List<PayslipQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipQuotaStore(Dictionary<string, PayslipQuota> items, List<string> order) : IPayslipQuotaStore
{
    public Dictionary<string, PayslipQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipQuota item)
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

    public List<PayslipQuota> List()
    {
        var outValue = new List<PayslipQuota>();
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
public sealed class CachedPayslipQuotaStore(MemoryPayslipQuotaStore inner, PayslipQuotaCache cache, int hits, int misses) : IPayslipQuotaStore
{
    public MemoryPayslipQuotaStore Inner { get; set; } = inner;
    public PayslipQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipQuota? Find(string code)
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

    public void Save(PayslipQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip quotas. Empty fields match all.</summary>
public sealed record PayslipQuotaQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipQuota item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipQuota> Apply(List<PayslipQuota> items)
    {
        var matched = Collections.Filter(items, (PayslipQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip quota store.</summary>
public static class PayslipQuotaStore
{
    public static IPayslipQuotaStore NewMemoryPayslipQuotaStore()
    {
        return new MemoryPayslipQuotaStore(new Dictionary<string, PayslipQuota>(), new List<string>());
    }

    public static IPayslipQuotaStore NewCachedPayslipQuotaStore(MemoryPayslipQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipQuotaStore(inner, PayslipQuotaCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayslipQuota> FindPayslipQuotas(IPayslipQuotaStore store, PayslipQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
