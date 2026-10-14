namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback return plan quotas by code.</summary>
public interface IFallbackReturnPlanQuotaStore
{
    FallbackReturnPlanQuota? Find(string code);
    void Save(FallbackReturnPlanQuota item);
    bool Remove(string code);
    List<FallbackReturnPlanQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackReturnPlanQuotaStore(Dictionary<string, FallbackReturnPlanQuota> items, List<string> order) : IFallbackReturnPlanQuotaStore
{
    public Dictionary<string, FallbackReturnPlanQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackReturnPlanQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackReturnPlanQuota item)
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

    public List<FallbackReturnPlanQuota> List()
    {
        var outValue = new List<FallbackReturnPlanQuota>();
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
public sealed class CachedFallbackReturnPlanQuotaStore(MemoryFallbackReturnPlanQuotaStore inner, FallbackReturnPlanQuotaCache cache, int hits, int misses) : IFallbackReturnPlanQuotaStore
{
    public MemoryFallbackReturnPlanQuotaStore Inner { get; set; } = inner;
    public FallbackReturnPlanQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackReturnPlanQuota? Find(string code)
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

    public void Save(FallbackReturnPlanQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackReturnPlanQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback return plan quotas. Empty fields match all.</summary>
public sealed record FallbackReturnPlanQuotaQuery(string Segment, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackReturnPlanQuota item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<FallbackReturnPlanQuota> Apply(List<FallbackReturnPlanQuota> items)
    {
        var matched = Collections.Filter(items, (FallbackReturnPlanQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback return plan quota store.</summary>
public static class FallbackReturnPlanQuotaStore
{
    public static IFallbackReturnPlanQuotaStore NewMemoryFallbackReturnPlanQuotaStore()
    {
        return new MemoryFallbackReturnPlanQuotaStore(
            new Dictionary<string, FallbackReturnPlanQuota>(),
            new List<string>());
    }

    public static IFallbackReturnPlanQuotaStore NewCachedFallbackReturnPlanQuotaStore(MemoryFallbackReturnPlanQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackReturnPlanQuotaStore(
            inner,
            FallbackReturnPlanQuotaCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackReturnPlanQuota> FindFallbackReturnPlanQuotas(IFallbackReturnPlanQuotaStore store, FallbackReturnPlanQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
