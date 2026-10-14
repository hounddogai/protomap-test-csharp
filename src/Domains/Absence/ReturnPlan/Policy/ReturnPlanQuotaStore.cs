namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan quotas by code.</summary>
public interface IReturnPlanQuotaStore
{
    ReturnPlanQuota? Find(string code);
    void Save(ReturnPlanQuota item);
    bool Remove(string code);
    List<ReturnPlanQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanQuotaStore(Dictionary<string, ReturnPlanQuota> items, List<string> order) : IReturnPlanQuotaStore
{
    public Dictionary<string, ReturnPlanQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanQuota item)
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

    public List<ReturnPlanQuota> List()
    {
        var outValue = new List<ReturnPlanQuota>();
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
public sealed class CachedReturnPlanQuotaStore(MemoryReturnPlanQuotaStore inner, ReturnPlanQuotaCache cache, int hits, int misses) : IReturnPlanQuotaStore
{
    public MemoryReturnPlanQuotaStore Inner { get; set; } = inner;
    public ReturnPlanQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanQuota? Find(string code)
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

    public void Save(ReturnPlanQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan quotas. Empty fields match all.</summary>
public sealed record ReturnPlanQuotaQuery(string Name, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanQuota item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanQuota> Apply(List<ReturnPlanQuota> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan quota store.</summary>
public static class ReturnPlanQuotaStore
{
    public static IReturnPlanQuotaStore NewMemoryReturnPlanQuotaStore()
    {
        return new MemoryReturnPlanQuotaStore(new Dictionary<string, ReturnPlanQuota>(), new List<string>());
    }

    public static IReturnPlanQuotaStore NewCachedReturnPlanQuotaStore(MemoryReturnPlanQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanQuotaStore(inner, ReturnPlanQuotaCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanQuota> FindReturnPlanQuotas(IReturnPlanQuotaStore store, ReturnPlanQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
