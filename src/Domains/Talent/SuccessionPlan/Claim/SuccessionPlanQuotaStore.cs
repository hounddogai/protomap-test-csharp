namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan quotas by code.</summary>
public interface ISuccessionPlanQuotaStore
{
    SuccessionPlanQuota? Find(string code);
    void Save(SuccessionPlanQuota item);
    bool Remove(string code);
    List<SuccessionPlanQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanQuotaStore(Dictionary<string, SuccessionPlanQuota> items, List<string> order) : ISuccessionPlanQuotaStore
{
    public Dictionary<string, SuccessionPlanQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanQuota item)
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

    public List<SuccessionPlanQuota> List()
    {
        var outValue = new List<SuccessionPlanQuota>();
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
public sealed class CachedSuccessionPlanQuotaStore(MemorySuccessionPlanQuotaStore inner, SuccessionPlanQuotaCache cache, int hits, int misses) : ISuccessionPlanQuotaStore
{
    public MemorySuccessionPlanQuotaStore Inner { get; set; } = inner;
    public SuccessionPlanQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanQuota? Find(string code)
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

    public void Save(SuccessionPlanQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan quotas. Empty fields match all.</summary>
public sealed record SuccessionPlanQuotaQuery(string Department, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanQuota item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanQuota> Apply(List<SuccessionPlanQuota> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan quota store.</summary>
public static class SuccessionPlanQuotaStore
{
    public static ISuccessionPlanQuotaStore NewMemorySuccessionPlanQuotaStore()
    {
        return new MemorySuccessionPlanQuotaStore(new Dictionary<string, SuccessionPlanQuota>(), new List<string>());
    }

    public static ISuccessionPlanQuotaStore NewCachedSuccessionPlanQuotaStore(MemorySuccessionPlanQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanQuotaStore(inner, SuccessionPlanQuotaCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanQuota> FindSuccessionPlanQuotas(ISuccessionPlanQuotaStore store, SuccessionPlanQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
