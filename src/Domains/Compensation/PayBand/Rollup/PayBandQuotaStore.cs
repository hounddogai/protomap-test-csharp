namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band quotas by code.</summary>
public interface IPayBandQuotaStore
{
    PayBandQuota? Find(string code);
    void Save(PayBandQuota item);
    bool Remove(string code);
    List<PayBandQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandQuotaStore(Dictionary<string, PayBandQuota> items, List<string> order) : IPayBandQuotaStore
{
    public Dictionary<string, PayBandQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandQuota item)
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

    public List<PayBandQuota> List()
    {
        var outValue = new List<PayBandQuota>();
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
public sealed class CachedPayBandQuotaStore(MemoryPayBandQuotaStore inner, PayBandQuotaCache cache, int hits, int misses) : IPayBandQuotaStore
{
    public MemoryPayBandQuotaStore Inner { get; set; } = inner;
    public PayBandQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandQuota? Find(string code)
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

    public void Save(PayBandQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band quotas. Empty fields match all.</summary>
public sealed record PayBandQuotaQuery(string Region, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandQuota item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<PayBandQuota> Apply(List<PayBandQuota> items)
    {
        var matched = Collections.Filter(items, (PayBandQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band quota store.</summary>
public static class PayBandQuotaStore
{
    public static IPayBandQuotaStore NewMemoryPayBandQuotaStore()
    {
        return new MemoryPayBandQuotaStore(new Dictionary<string, PayBandQuota>(), new List<string>());
    }

    public static IPayBandQuotaStore NewCachedPayBandQuotaStore(MemoryPayBandQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandQuotaStore(inner, PayBandQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayBandQuota> FindPayBandQuotas(IPayBandQuotaStore store, PayBandQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
