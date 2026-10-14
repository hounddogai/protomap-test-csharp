namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount quotas by code.</summary>
public interface IHeadcountQuotaStore
{
    HeadcountQuota? Find(string code);
    void Save(HeadcountQuota item);
    bool Remove(string code);
    List<HeadcountQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountQuotaStore(Dictionary<string, HeadcountQuota> items, List<string> order) : IHeadcountQuotaStore
{
    public Dictionary<string, HeadcountQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountQuota item)
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

    public List<HeadcountQuota> List()
    {
        var outValue = new List<HeadcountQuota>();
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
public sealed class CachedHeadcountQuotaStore(MemoryHeadcountQuotaStore inner, HeadcountQuotaCache cache, int hits, int misses) : IHeadcountQuotaStore
{
    public MemoryHeadcountQuotaStore Inner { get; set; } = inner;
    public HeadcountQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountQuota? Find(string code)
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

    public void Save(HeadcountQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount quotas. Empty fields match all.</summary>
public sealed record HeadcountQuotaQuery(string Channel, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountQuota item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountQuota> Apply(List<HeadcountQuota> items)
    {
        var matched = Collections.Filter(items, (HeadcountQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount quota store.</summary>
public static class HeadcountQuotaStore
{
    public static IHeadcountQuotaStore NewMemoryHeadcountQuotaStore()
    {
        return new MemoryHeadcountQuotaStore(new Dictionary<string, HeadcountQuota>(), new List<string>());
    }

    public static IHeadcountQuotaStore NewCachedHeadcountQuotaStore(MemoryHeadcountQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountQuotaStore(inner, HeadcountQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<HeadcountQuota> FindHeadcountQuotas(IHeadcountQuotaStore store, HeadcountQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
