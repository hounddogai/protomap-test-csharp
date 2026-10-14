namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback warranty quotas by code.</summary>
public interface IFallbackWarrantyQuotaStore
{
    FallbackWarrantyQuota? Find(string code);
    void Save(FallbackWarrantyQuota item);
    bool Remove(string code);
    List<FallbackWarrantyQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackWarrantyQuotaStore(Dictionary<string, FallbackWarrantyQuota> items, List<string> order) : IFallbackWarrantyQuotaStore
{
    public Dictionary<string, FallbackWarrantyQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackWarrantyQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackWarrantyQuota item)
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

    public List<FallbackWarrantyQuota> List()
    {
        var outValue = new List<FallbackWarrantyQuota>();
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
public sealed class CachedFallbackWarrantyQuotaStore(MemoryFallbackWarrantyQuotaStore inner, FallbackWarrantyQuotaCache cache, int hits, int misses) : IFallbackWarrantyQuotaStore
{
    public MemoryFallbackWarrantyQuotaStore Inner { get; set; } = inner;
    public FallbackWarrantyQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackWarrantyQuota? Find(string code)
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

    public void Save(FallbackWarrantyQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackWarrantyQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback warranty quotas. Empty fields match all.</summary>
public sealed record FallbackWarrantyQuotaQuery(string Source, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackWarrantyQuota item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackWarrantyQuota> Apply(List<FallbackWarrantyQuota> items)
    {
        var matched = Collections.Filter(items, (FallbackWarrantyQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback warranty quota store.</summary>
public static class FallbackWarrantyQuotaStore
{
    public static IFallbackWarrantyQuotaStore NewMemoryFallbackWarrantyQuotaStore()
    {
        return new MemoryFallbackWarrantyQuotaStore(
            new Dictionary<string, FallbackWarrantyQuota>(),
            new List<string>());
    }

    public static IFallbackWarrantyQuotaStore NewCachedFallbackWarrantyQuotaStore(MemoryFallbackWarrantyQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackWarrantyQuotaStore(inner, FallbackWarrantyQuotaCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FallbackWarrantyQuota> FindFallbackWarrantyQuotas(IFallbackWarrantyQuotaStore store, FallbackWarrantyQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
