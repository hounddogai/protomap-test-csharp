namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback succession plan quotas by code.</summary>
public interface IFallbackSuccessionPlanQuotaStore
{
    FallbackSuccessionPlanQuota? Find(string code);
    void Save(FallbackSuccessionPlanQuota item);
    bool Remove(string code);
    List<FallbackSuccessionPlanQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackSuccessionPlanQuotaStore(Dictionary<string, FallbackSuccessionPlanQuota> items, List<string> order) : IFallbackSuccessionPlanQuotaStore
{
    public Dictionary<string, FallbackSuccessionPlanQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackSuccessionPlanQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackSuccessionPlanQuota item)
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

    public List<FallbackSuccessionPlanQuota> List()
    {
        var outValue = new List<FallbackSuccessionPlanQuota>();
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
public sealed class CachedFallbackSuccessionPlanQuotaStore(MemoryFallbackSuccessionPlanQuotaStore inner, FallbackSuccessionPlanQuotaCache cache, int hits, int misses) : IFallbackSuccessionPlanQuotaStore
{
    public MemoryFallbackSuccessionPlanQuotaStore Inner { get; set; } = inner;
    public FallbackSuccessionPlanQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackSuccessionPlanQuota? Find(string code)
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

    public void Save(FallbackSuccessionPlanQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackSuccessionPlanQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback succession plan quotas. Empty fields match all.</summary>
public sealed record FallbackSuccessionPlanQuotaQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackSuccessionPlanQuota item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackSuccessionPlanQuota> Apply(List<FallbackSuccessionPlanQuota> items)
    {
        var matched = Collections.Filter(items, (FallbackSuccessionPlanQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback succession plan quota store.</summary>
public static class FallbackSuccessionPlanQuotaStore
{
    public static IFallbackSuccessionPlanQuotaStore NewMemoryFallbackSuccessionPlanQuotaStore()
    {
        return new MemoryFallbackSuccessionPlanQuotaStore(
            new Dictionary<string, FallbackSuccessionPlanQuota>(),
            new List<string>());
    }

    public static IFallbackSuccessionPlanQuotaStore NewCachedFallbackSuccessionPlanQuotaStore(MemoryFallbackSuccessionPlanQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackSuccessionPlanQuotaStore(
            inner,
            FallbackSuccessionPlanQuotaCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackSuccessionPlanQuota> FindFallbackSuccessionPlanQuotas(IFallbackSuccessionPlanQuotaStore store, FallbackSuccessionPlanQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
