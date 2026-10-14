namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default return plan quotas by code.</summary>
public interface IDefaultReturnPlanQuotaStore
{
    DefaultReturnPlanQuota? Find(string code);
    void Save(DefaultReturnPlanQuota item);
    bool Remove(string code);
    List<DefaultReturnPlanQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultReturnPlanQuotaStore(Dictionary<string, DefaultReturnPlanQuota> items, List<string> order) : IDefaultReturnPlanQuotaStore
{
    public Dictionary<string, DefaultReturnPlanQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultReturnPlanQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultReturnPlanQuota item)
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

    public List<DefaultReturnPlanQuota> List()
    {
        var outValue = new List<DefaultReturnPlanQuota>();
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
public sealed class CachedDefaultReturnPlanQuotaStore(MemoryDefaultReturnPlanQuotaStore inner, DefaultReturnPlanQuotaCache cache, int hits, int misses) : IDefaultReturnPlanQuotaStore
{
    public MemoryDefaultReturnPlanQuotaStore Inner { get; set; } = inner;
    public DefaultReturnPlanQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultReturnPlanQuota? Find(string code)
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

    public void Save(DefaultReturnPlanQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultReturnPlanQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default return plan quotas. Empty fields match all.</summary>
public sealed record DefaultReturnPlanQuotaQuery(string Name, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultReturnPlanQuota item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<DefaultReturnPlanQuota> Apply(List<DefaultReturnPlanQuota> items)
    {
        var matched = Collections.Filter(items, (DefaultReturnPlanQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default return plan quota store.</summary>
public static class DefaultReturnPlanQuotaStore
{
    public static IDefaultReturnPlanQuotaStore NewMemoryDefaultReturnPlanQuotaStore()
    {
        return new MemoryDefaultReturnPlanQuotaStore(
            new Dictionary<string, DefaultReturnPlanQuota>(),
            new List<string>());
    }

    public static IDefaultReturnPlanQuotaStore NewCachedDefaultReturnPlanQuotaStore(MemoryDefaultReturnPlanQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultReturnPlanQuotaStore(
            inner,
            DefaultReturnPlanQuotaCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultReturnPlanQuota> FindDefaultReturnPlanQuotas(IDefaultReturnPlanQuotaStore store, DefaultReturnPlanQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
