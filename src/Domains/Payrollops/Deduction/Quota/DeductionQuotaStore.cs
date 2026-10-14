namespace Acme.Workforce.Payrollops.Deduction.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction quotas by code.</summary>
public interface IDeductionQuotaStore
{
    DeductionQuota? Find(string code);
    void Save(DeductionQuota item);
    bool Remove(string code);
    List<DeductionQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionQuotaStore(Dictionary<string, DeductionQuota> items, List<string> order) : IDeductionQuotaStore
{
    public Dictionary<string, DeductionQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionQuota item)
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

    public List<DeductionQuota> List()
    {
        var outValue = new List<DeductionQuota>();
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
public sealed class CachedDeductionQuotaStore(MemoryDeductionQuotaStore inner, DeductionQuotaCache cache, int hits, int misses) : IDeductionQuotaStore
{
    public MemoryDeductionQuotaStore Inner { get; set; } = inner;
    public DeductionQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionQuota? Find(string code)
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

    public void Save(DeductionQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction quotas. Empty fields match all.</summary>
public sealed record DeductionQuotaQuery(string Country, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionQuota item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionQuota> Apply(List<DeductionQuota> items)
    {
        var matched = Collections.Filter(items, (DeductionQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction quota store.</summary>
public static class DeductionQuotaStore
{
    public static IDeductionQuotaStore NewMemoryDeductionQuotaStore()
    {
        return new MemoryDeductionQuotaStore(new Dictionary<string, DeductionQuota>(), new List<string>());
    }

    public static IDeductionQuotaStore NewCachedDeductionQuotaStore(MemoryDeductionQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionQuotaStore(inner, DeductionQuotaCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DeductionQuota> FindDeductionQuotas(IDeductionQuotaStore store, DeductionQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
