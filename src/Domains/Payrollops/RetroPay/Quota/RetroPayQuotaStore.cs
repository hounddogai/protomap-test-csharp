namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay quotas by code.</summary>
public interface IRetroPayQuotaStore
{
    RetroPayQuota? Find(string code);
    void Save(RetroPayQuota item);
    bool Remove(string code);
    List<RetroPayQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayQuotaStore(Dictionary<string, RetroPayQuota> items, List<string> order) : IRetroPayQuotaStore
{
    public Dictionary<string, RetroPayQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayQuota item)
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

    public List<RetroPayQuota> List()
    {
        var outValue = new List<RetroPayQuota>();
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
public sealed class CachedRetroPayQuotaStore(MemoryRetroPayQuotaStore inner, RetroPayQuotaCache cache, int hits, int misses) : IRetroPayQuotaStore
{
    public MemoryRetroPayQuotaStore Inner { get; set; } = inner;
    public RetroPayQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayQuota? Find(string code)
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

    public void Save(RetroPayQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay quotas. Empty fields match all.</summary>
public sealed record RetroPayQuotaQuery(string Name, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayQuota item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayQuota> Apply(List<RetroPayQuota> items)
    {
        var matched = Collections.Filter(items, (RetroPayQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay quota store.</summary>
public static class RetroPayQuotaStore
{
    public static IRetroPayQuotaStore NewMemoryRetroPayQuotaStore()
    {
        return new MemoryRetroPayQuotaStore(new Dictionary<string, RetroPayQuota>(), new List<string>());
    }

    public static IRetroPayQuotaStore NewCachedRetroPayQuotaStore(MemoryRetroPayQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayQuotaStore(inner, RetroPayQuotaCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RetroPayQuota> FindRetroPayQuotas(IRetroPayQuotaStore store, RetroPayQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
