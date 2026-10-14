namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual pulse survey quotas by code.</summary>
public interface IManualPulseSurveyQuotaStore
{
    ManualPulseSurveyQuota? Find(string code);
    void Save(ManualPulseSurveyQuota item);
    bool Remove(string code);
    List<ManualPulseSurveyQuota> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPulseSurveyQuotaStore(Dictionary<string, ManualPulseSurveyQuota> items, List<string> order) : IManualPulseSurveyQuotaStore
{
    public Dictionary<string, ManualPulseSurveyQuota> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPulseSurveyQuota? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPulseSurveyQuota item)
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

    public List<ManualPulseSurveyQuota> List()
    {
        var outValue = new List<ManualPulseSurveyQuota>();
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
public sealed class CachedManualPulseSurveyQuotaStore(MemoryManualPulseSurveyQuotaStore inner, ManualPulseSurveyQuotaCache cache, int hits, int misses) : IManualPulseSurveyQuotaStore
{
    public MemoryManualPulseSurveyQuotaStore Inner { get; set; } = inner;
    public ManualPulseSurveyQuotaCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPulseSurveyQuota? Find(string code)
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

    public void Save(ManualPulseSurveyQuota item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPulseSurveyQuota> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual pulse survey quotas. Empty fields match all.</summary>
public sealed record ManualPulseSurveyQuotaQuery(string Region, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPulseSurveyQuota item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualPulseSurveyQuota> Apply(List<ManualPulseSurveyQuota> items)
    {
        var matched = Collections.Filter(items, (ManualPulseSurveyQuota candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual pulse survey quota store.</summary>
public static class ManualPulseSurveyQuotaStore
{
    public static IManualPulseSurveyQuotaStore NewMemoryManualPulseSurveyQuotaStore()
    {
        return new MemoryManualPulseSurveyQuotaStore(
            new Dictionary<string, ManualPulseSurveyQuota>(),
            new List<string>());
    }

    public static IManualPulseSurveyQuotaStore NewCachedManualPulseSurveyQuotaStore(MemoryManualPulseSurveyQuotaStore inner, int ttl, IClock clock)
    {
        return new CachedManualPulseSurveyQuotaStore(
            inner,
            ManualPulseSurveyQuotaCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ManualPulseSurveyQuota> FindManualPulseSurveyQuotas(IManualPulseSurveyQuotaStore store, ManualPulseSurveyQuotaQuery query)
    {
        return query.Apply(store.List());
    }
}
