namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment quotas by code.</summary>
public interface IAdjustmentQuotaStore
{
    AdjustmentQuota? Find(string code);
    void Save(AdjustmentQuota item);
    bool Remove(string code);
    List<AdjustmentQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentQuotaStore(Dictionary<string, AdjustmentQuota> items, List<string> order) : IAdjustmentQuotaStore
{
    public Dictionary<string, AdjustmentQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentQuota item)
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

    public List<AdjustmentQuota> List()
    {
        var outValue = new List<AdjustmentQuota>();
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
public sealed class CachedAdjustmentQuotaStore(MemoryAdjustmentQuotaStore inner, AdjustmentQuotaCache cache, int hits, int misses) : IAdjustmentQuotaStore
{
    public MemoryAdjustmentQuotaStore Inner { get; set; } = inner;
    public AdjustmentQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentQuota? Find(string code)
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

    public void Save(AdjustmentQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment quotas. Empty fields match all.</summary>
public sealed record AdjustmentQuotaQuery(string Name, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentQuota item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentQuota> Apply(List<AdjustmentQuota> items)
    {
        var matched = Collections.Filter(items, (AdjustmentQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment quota store.</summary>
public static class AdjustmentQuotaStore
{
    public static IAdjustmentQuotaStore NewMemoryAdjustmentQuotaStore()
    {
        return new MemoryAdjustmentQuotaStore(new Dictionary<string, AdjustmentQuota>(), new List<string>());
    }

    public static IAdjustmentQuotaStore NewCachedAdjustmentQuotaStore(MemoryAdjustmentQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentQuotaStore(inner, AdjustmentQuotaCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AdjustmentQuota> FindAdjustmentQuotas(IAdjustmentQuotaStore store, AdjustmentQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
