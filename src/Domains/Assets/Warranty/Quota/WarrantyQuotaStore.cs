namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty quotas by code.</summary>
public interface IWarrantyQuotaStore
{
    WarrantyQuota? Find(string code);
    void Save(WarrantyQuota item);
    bool Remove(string code);
    List<WarrantyQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyQuotaStore(Dictionary<string, WarrantyQuota> items, List<string> order) : IWarrantyQuotaStore
{
    public Dictionary<string, WarrantyQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyQuota item)
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

    public List<WarrantyQuota> List()
    {
        var outValue = new List<WarrantyQuota>();
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
public sealed class CachedWarrantyQuotaStore(MemoryWarrantyQuotaStore inner, WarrantyQuotaCache cache, int hits, int misses) : IWarrantyQuotaStore
{
    public MemoryWarrantyQuotaStore Inner { get; set; } = inner;
    public WarrantyQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyQuota? Find(string code)
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

    public void Save(WarrantyQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty quotas. Empty fields match all.</summary>
public sealed record WarrantyQuotaQuery(string Unit, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyQuota item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<WarrantyQuota> Apply(List<WarrantyQuota> items)
    {
        var matched = Collections.Filter(items, (WarrantyQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty quota store.</summary>
public static class WarrantyQuotaStore
{
    public static IWarrantyQuotaStore NewMemoryWarrantyQuotaStore()
    {
        return new MemoryWarrantyQuotaStore(new Dictionary<string, WarrantyQuota>(), new List<string>());
    }

    public static IWarrantyQuotaStore NewCachedWarrantyQuotaStore(MemoryWarrantyQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyQuotaStore(inner, WarrantyQuotaCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantyQuota> FindWarrantyQuotas(IWarrantyQuotaStore store, WarrantyQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
