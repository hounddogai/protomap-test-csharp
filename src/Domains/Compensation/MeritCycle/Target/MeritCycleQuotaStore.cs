namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle quotas by code.</summary>
public interface IMeritCycleQuotaStore
{
    MeritCycleQuota? Find(string code);
    void Save(MeritCycleQuota item);
    bool Remove(string code);
    List<MeritCycleQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleQuotaStore(Dictionary<string, MeritCycleQuota> items, List<string> order) : IMeritCycleQuotaStore
{
    public Dictionary<string, MeritCycleQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleQuota item)
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

    public List<MeritCycleQuota> List()
    {
        var outValue = new List<MeritCycleQuota>();
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
public sealed class CachedMeritCycleQuotaStore(MemoryMeritCycleQuotaStore inner, MeritCycleQuotaCache cache, int hits, int misses) : IMeritCycleQuotaStore
{
    public MemoryMeritCycleQuotaStore Inner { get; set; } = inner;
    public MeritCycleQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleQuota? Find(string code)
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

    public void Save(MeritCycleQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle quotas. Empty fields match all.</summary>
public sealed record MeritCycleQuotaQuery(string Category, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleQuota item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleQuota> Apply(List<MeritCycleQuota> items)
    {
        var matched = Collections.Filter(items, (MeritCycleQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle quota store.</summary>
public static class MeritCycleQuotaStore
{
    public static IMeritCycleQuotaStore NewMemoryMeritCycleQuotaStore()
    {
        return new MemoryMeritCycleQuotaStore(new Dictionary<string, MeritCycleQuota>(), new List<string>());
    }

    public static IMeritCycleQuotaStore NewCachedMeritCycleQuotaStore(MemoryMeritCycleQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleQuotaStore(inner, MeritCycleQuotaCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<MeritCycleQuota> FindMeritCycleQuotas(IMeritCycleQuotaStore store, MeritCycleQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
