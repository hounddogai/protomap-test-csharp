namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback position quotas by code.</summary>
public interface IFallbackPositionQuotaStore
{
    FallbackPositionQuota? Find(string code);
    void Save(FallbackPositionQuota item);
    bool Remove(string code);
    List<FallbackPositionQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPositionQuotaStore(Dictionary<string, FallbackPositionQuota> items, List<string> order) : IFallbackPositionQuotaStore
{
    public Dictionary<string, FallbackPositionQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPositionQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPositionQuota item)
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

    public List<FallbackPositionQuota> List()
    {
        var outValue = new List<FallbackPositionQuota>();
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
public sealed class CachedFallbackPositionQuotaStore(MemoryFallbackPositionQuotaStore inner, FallbackPositionQuotaCache cache, int hits, int misses) : IFallbackPositionQuotaStore
{
    public MemoryFallbackPositionQuotaStore Inner { get; set; } = inner;
    public FallbackPositionQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPositionQuota? Find(string code)
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

    public void Save(FallbackPositionQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPositionQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback position quotas. Empty fields match all.</summary>
public sealed record FallbackPositionQuotaQuery(string Region, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPositionQuota item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPositionQuota> Apply(List<FallbackPositionQuota> items)
    {
        var matched = Collections.Filter(items, (FallbackPositionQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback position quota store.</summary>
public static class FallbackPositionQuotaStore
{
    public static IFallbackPositionQuotaStore NewMemoryFallbackPositionQuotaStore()
    {
        return new MemoryFallbackPositionQuotaStore(
            new Dictionary<string, FallbackPositionQuota>(),
            new List<string>());
    }

    public static IFallbackPositionQuotaStore NewCachedFallbackPositionQuotaStore(MemoryFallbackPositionQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPositionQuotaStore(
            inner,
            FallbackPositionQuotaCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackPositionQuota> FindFallbackPositionQuotas(IFallbackPositionQuotaStore store, FallbackPositionQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
