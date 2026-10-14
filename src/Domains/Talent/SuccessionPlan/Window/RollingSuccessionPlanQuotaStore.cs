namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling succession plan quotas by code.</summary>
public interface IRollingSuccessionPlanQuotaStore
{
    RollingSuccessionPlanQuota? Find(string code);
    void Save(RollingSuccessionPlanQuota item);
    bool Remove(string code);
    List<RollingSuccessionPlanQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingSuccessionPlanQuotaStore(Dictionary<string, RollingSuccessionPlanQuota> items, List<string> order) : IRollingSuccessionPlanQuotaStore
{
    public Dictionary<string, RollingSuccessionPlanQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingSuccessionPlanQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingSuccessionPlanQuota item)
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

    public List<RollingSuccessionPlanQuota> List()
    {
        var outValue = new List<RollingSuccessionPlanQuota>();
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
public sealed class CachedRollingSuccessionPlanQuotaStore(MemoryRollingSuccessionPlanQuotaStore inner, RollingSuccessionPlanQuotaCache cache, int hits, int misses) : IRollingSuccessionPlanQuotaStore
{
    public MemoryRollingSuccessionPlanQuotaStore Inner { get; set; } = inner;
    public RollingSuccessionPlanQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingSuccessionPlanQuota? Find(string code)
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

    public void Save(RollingSuccessionPlanQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingSuccessionPlanQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling succession plan quotas. Empty fields match all.</summary>
public sealed record RollingSuccessionPlanQuotaQuery(string Source, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(RollingSuccessionPlanQuota item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingSuccessionPlanQuota> Apply(List<RollingSuccessionPlanQuota> items)
    {
        var matched = Collections.Filter(items, (RollingSuccessionPlanQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling succession plan quota store.</summary>
public static class RollingSuccessionPlanQuotaStore
{
    public static IRollingSuccessionPlanQuotaStore NewMemoryRollingSuccessionPlanQuotaStore()
    {
        return new MemoryRollingSuccessionPlanQuotaStore(
            new Dictionary<string, RollingSuccessionPlanQuota>(),
            new List<string>());
    }

    public static IRollingSuccessionPlanQuotaStore NewCachedRollingSuccessionPlanQuotaStore(MemoryRollingSuccessionPlanQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedRollingSuccessionPlanQuotaStore(
            inner,
            RollingSuccessionPlanQuotaCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RollingSuccessionPlanQuota> FindRollingSuccessionPlanQuotas(IRollingSuccessionPlanQuotaStore store, RollingSuccessionPlanQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
