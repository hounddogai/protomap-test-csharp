namespace Acme.Workforce.Absence.Policy.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback policy quotas by code.</summary>
public interface IFallbackPolicyQuotaStore
{
    FallbackPolicyQuota? Find(string code);
    void Save(FallbackPolicyQuota item);
    bool Remove(string code);
    List<FallbackPolicyQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPolicyQuotaStore(Dictionary<string, FallbackPolicyQuota> items, List<string> order) : IFallbackPolicyQuotaStore
{
    public Dictionary<string, FallbackPolicyQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPolicyQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPolicyQuota item)
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

    public List<FallbackPolicyQuota> List()
    {
        var outValue = new List<FallbackPolicyQuota>();
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
public sealed class CachedFallbackPolicyQuotaStore(MemoryFallbackPolicyQuotaStore inner, FallbackPolicyQuotaCache cache, int hits, int misses) : IFallbackPolicyQuotaStore
{
    public MemoryFallbackPolicyQuotaStore Inner { get; set; } = inner;
    public FallbackPolicyQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPolicyQuota? Find(string code)
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

    public void Save(FallbackPolicyQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPolicyQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback policy quotas. Empty fields match all.</summary>
public sealed record FallbackPolicyQuotaQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPolicyQuota item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPolicyQuota> Apply(List<FallbackPolicyQuota> items)
    {
        var matched = Collections.Filter(items, (FallbackPolicyQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback policy quota store.</summary>
public static class FallbackPolicyQuotaStore
{
    public static IFallbackPolicyQuotaStore NewMemoryFallbackPolicyQuotaStore()
    {
        return new MemoryFallbackPolicyQuotaStore(new Dictionary<string, FallbackPolicyQuota>(), new List<string>());
    }

    public static IFallbackPolicyQuotaStore NewCachedFallbackPolicyQuotaStore(MemoryFallbackPolicyQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPolicyQuotaStore(inner, FallbackPolicyQuotaCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FallbackPolicyQuota> FindFallbackPolicyQuotas(IFallbackPolicyQuotaStore store, FallbackPolicyQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
