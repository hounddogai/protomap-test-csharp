namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate quotas by code.</summary>
public interface IRateQuotaStore
{
    RateQuota? Find(string code);
    void Save(RateQuota item);
    bool Remove(string code);
    List<RateQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateQuotaStore(Dictionary<string, RateQuota> items, List<string> order) : IRateQuotaStore
{
    public Dictionary<string, RateQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateQuota item)
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

    public List<RateQuota> List()
    {
        var outValue = new List<RateQuota>();
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
public sealed class CachedRateQuotaStore(MemoryRateQuotaStore inner, RateQuotaCache cache, int hits, int misses) : IRateQuotaStore
{
    public MemoryRateQuotaStore Inner { get; set; } = inner;
    public RateQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateQuota? Find(string code)
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

    public void Save(RateQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate quotas. Empty fields match all.</summary>
public sealed record RateQuotaQuery(string Department, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(RateQuota item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateQuota> Apply(List<RateQuota> items)
    {
        var matched = Collections.Filter(items, (RateQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate quota store.</summary>
public static class RateQuotaStore
{
    public static IRateQuotaStore NewMemoryRateQuotaStore()
    {
        return new MemoryRateQuotaStore(new Dictionary<string, RateQuota>(), new List<string>());
    }

    public static IRateQuotaStore NewCachedRateQuotaStore(MemoryRateQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedRateQuotaStore(inner, RateQuotaCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RateQuota> FindRateQuotas(IRateQuotaStore store, RateQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
