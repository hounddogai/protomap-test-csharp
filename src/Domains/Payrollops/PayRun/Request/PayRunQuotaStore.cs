namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run quotas by code.</summary>
public interface IPayRunQuotaStore
{
    PayRunQuota? Find(string code);
    void Save(PayRunQuota item);
    bool Remove(string code);
    List<PayRunQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunQuotaStore(Dictionary<string, PayRunQuota> items, List<string> order) : IPayRunQuotaStore
{
    public Dictionary<string, PayRunQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunQuota item)
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

    public List<PayRunQuota> List()
    {
        var outValue = new List<PayRunQuota>();
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
public sealed class CachedPayRunQuotaStore(MemoryPayRunQuotaStore inner, PayRunQuotaCache cache, int hits, int misses) : IPayRunQuotaStore
{
    public MemoryPayRunQuotaStore Inner { get; set; } = inner;
    public PayRunQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunQuota? Find(string code)
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

    public void Save(PayRunQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run quotas. Empty fields match all.</summary>
public sealed record PayRunQuotaQuery(string Channel, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunQuota item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunQuota> Apply(List<PayRunQuota> items)
    {
        var matched = Collections.Filter(items, (PayRunQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run quota store.</summary>
public static class PayRunQuotaStore
{
    public static IPayRunQuotaStore NewMemoryPayRunQuotaStore()
    {
        return new MemoryPayRunQuotaStore(new Dictionary<string, PayRunQuota>(), new List<string>());
    }

    public static IPayRunQuotaStore NewCachedPayRunQuotaStore(MemoryPayRunQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunQuotaStore(inner, PayRunQuotaCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayRunQuota> FindPayRunQuotas(IPayRunQuotaStore store, PayRunQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
