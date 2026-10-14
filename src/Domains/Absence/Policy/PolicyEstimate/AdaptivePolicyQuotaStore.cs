namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive policy quotas by code.</summary>
public interface IAdaptivePolicyQuotaStore
{
    AdaptivePolicyQuota? Find(string code);
    void Save(AdaptivePolicyQuota item);
    bool Remove(string code);
    List<AdaptivePolicyQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptivePolicyQuotaStore(Dictionary<string, AdaptivePolicyQuota> items, List<string> order) : IAdaptivePolicyQuotaStore
{
    public Dictionary<string, AdaptivePolicyQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptivePolicyQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptivePolicyQuota item)
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

    public List<AdaptivePolicyQuota> List()
    {
        var outValue = new List<AdaptivePolicyQuota>();
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
public sealed class CachedAdaptivePolicyQuotaStore(MemoryAdaptivePolicyQuotaStore inner, AdaptivePolicyQuotaCache cache, int hits, int misses) : IAdaptivePolicyQuotaStore
{
    public MemoryAdaptivePolicyQuotaStore Inner { get; set; } = inner;
    public AdaptivePolicyQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptivePolicyQuota? Find(string code)
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

    public void Save(AdaptivePolicyQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptivePolicyQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive policy quotas. Empty fields match all.</summary>
public sealed record AdaptivePolicyQuotaQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptivePolicyQuota item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptivePolicyQuota> Apply(List<AdaptivePolicyQuota> items)
    {
        var matched = Collections.Filter(items, (AdaptivePolicyQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive policy quota store.</summary>
public static class AdaptivePolicyQuotaStore
{
    public static IAdaptivePolicyQuotaStore NewMemoryAdaptivePolicyQuotaStore()
    {
        return new MemoryAdaptivePolicyQuotaStore(new Dictionary<string, AdaptivePolicyQuota>(), new List<string>());
    }

    public static IAdaptivePolicyQuotaStore NewCachedAdaptivePolicyQuotaStore(MemoryAdaptivePolicyQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptivePolicyQuotaStore(inner, AdaptivePolicyQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdaptivePolicyQuota> FindAdaptivePolicyQuotas(IAdaptivePolicyQuotaStore store, AdaptivePolicyQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
