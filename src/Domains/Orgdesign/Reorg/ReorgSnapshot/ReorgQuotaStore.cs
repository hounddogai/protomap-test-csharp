namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg quotas by code.</summary>
public interface IReorgQuotaStore
{
    ReorgQuota? Find(string code);
    void Save(ReorgQuota item);
    bool Remove(string code);
    List<ReorgQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgQuotaStore(Dictionary<string, ReorgQuota> items, List<string> order) : IReorgQuotaStore
{
    public Dictionary<string, ReorgQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgQuota item)
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

    public List<ReorgQuota> List()
    {
        var outValue = new List<ReorgQuota>();
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
public sealed class CachedReorgQuotaStore(MemoryReorgQuotaStore inner, ReorgQuotaCache cache, int hits, int misses) : IReorgQuotaStore
{
    public MemoryReorgQuotaStore Inner { get; set; } = inner;
    public ReorgQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgQuota? Find(string code)
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

    public void Save(ReorgQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg quotas. Empty fields match all.</summary>
public sealed record ReorgQuotaQuery(string Channel, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgQuota item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgQuota> Apply(List<ReorgQuota> items)
    {
        var matched = Collections.Filter(items, (ReorgQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg quota store.</summary>
public static class ReorgQuotaStore
{
    public static IReorgQuotaStore NewMemoryReorgQuotaStore()
    {
        return new MemoryReorgQuotaStore(new Dictionary<string, ReorgQuota>(), new List<string>());
    }

    public static IReorgQuotaStore NewCachedReorgQuotaStore(MemoryReorgQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedReorgQuotaStore(inner, ReorgQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReorgQuota> FindReorgQuotas(IReorgQuotaStore store, ReorgQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
