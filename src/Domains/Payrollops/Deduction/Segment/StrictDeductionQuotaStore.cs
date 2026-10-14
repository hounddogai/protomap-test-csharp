namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict deduction quotas by code.</summary>
public interface IStrictDeductionQuotaStore
{
    StrictDeductionQuota? Find(string code);
    void Save(StrictDeductionQuota item);
    bool Remove(string code);
    List<StrictDeductionQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictDeductionQuotaStore(Dictionary<string, StrictDeductionQuota> items, List<string> order) : IStrictDeductionQuotaStore
{
    public Dictionary<string, StrictDeductionQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictDeductionQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictDeductionQuota item)
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

    public List<StrictDeductionQuota> List()
    {
        var outValue = new List<StrictDeductionQuota>();
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
public sealed class CachedStrictDeductionQuotaStore(MemoryStrictDeductionQuotaStore inner, StrictDeductionQuotaCache cache, int hits, int misses) : IStrictDeductionQuotaStore
{
    public MemoryStrictDeductionQuotaStore Inner { get; set; } = inner;
    public StrictDeductionQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictDeductionQuota? Find(string code)
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

    public void Save(StrictDeductionQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictDeductionQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict deduction quotas. Empty fields match all.</summary>
public sealed record StrictDeductionQuotaQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(StrictDeductionQuota item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictDeductionQuota> Apply(List<StrictDeductionQuota> items)
    {
        var matched = Collections.Filter(items, (StrictDeductionQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict deduction quota store.</summary>
public static class StrictDeductionQuotaStore
{
    public static IStrictDeductionQuotaStore NewMemoryStrictDeductionQuotaStore()
    {
        return new MemoryStrictDeductionQuotaStore(
            new Dictionary<string, StrictDeductionQuota>(),
            new List<string>());
    }

    public static IStrictDeductionQuotaStore NewCachedStrictDeductionQuotaStore(MemoryStrictDeductionQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedStrictDeductionQuotaStore(inner, StrictDeductionQuotaCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictDeductionQuota> FindStrictDeductionQuotas(IStrictDeductionQuotaStore store, StrictDeductionQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
