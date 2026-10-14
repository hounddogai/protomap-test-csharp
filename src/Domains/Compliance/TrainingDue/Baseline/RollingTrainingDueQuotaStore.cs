namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling training due quotas by code.</summary>
public interface IRollingTrainingDueQuotaStore
{
    RollingTrainingDueQuota? Find(string code);
    void Save(RollingTrainingDueQuota item);
    bool Remove(string code);
    List<RollingTrainingDueQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingTrainingDueQuotaStore(Dictionary<string, RollingTrainingDueQuota> items, List<string> order) : IRollingTrainingDueQuotaStore
{
    public Dictionary<string, RollingTrainingDueQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingTrainingDueQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingTrainingDueQuota item)
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

    public List<RollingTrainingDueQuota> List()
    {
        var outValue = new List<RollingTrainingDueQuota>();
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
public sealed class CachedRollingTrainingDueQuotaStore(MemoryRollingTrainingDueQuotaStore inner, RollingTrainingDueQuotaCache cache, int hits, int misses) : IRollingTrainingDueQuotaStore
{
    public MemoryRollingTrainingDueQuotaStore Inner { get; set; } = inner;
    public RollingTrainingDueQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingTrainingDueQuota? Find(string code)
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

    public void Save(RollingTrainingDueQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingTrainingDueQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling training due quotas. Empty fields match all.</summary>
public sealed record RollingTrainingDueQuotaQuery(string Country, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RollingTrainingDueQuota item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingTrainingDueQuota> Apply(List<RollingTrainingDueQuota> items)
    {
        var matched = Collections.Filter(items, (RollingTrainingDueQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling training due quota store.</summary>
public static class RollingTrainingDueQuotaStore
{
    public static IRollingTrainingDueQuotaStore NewMemoryRollingTrainingDueQuotaStore()
    {
        return new MemoryRollingTrainingDueQuotaStore(
            new Dictionary<string, RollingTrainingDueQuota>(),
            new List<string>());
    }

    public static IRollingTrainingDueQuotaStore NewCachedRollingTrainingDueQuotaStore(MemoryRollingTrainingDueQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedRollingTrainingDueQuotaStore(
            inner,
            RollingTrainingDueQuotaCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RollingTrainingDueQuota> FindRollingTrainingDueQuotas(IRollingTrainingDueQuotaStore store, RollingTrainingDueQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
