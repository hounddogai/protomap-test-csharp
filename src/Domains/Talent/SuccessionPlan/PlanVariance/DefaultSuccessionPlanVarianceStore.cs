namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default succession plan variances by code.</summary>
public interface IDefaultSuccessionPlanVarianceStore
{
    DefaultSuccessionPlanVariance? Find(string code);
    void Save(DefaultSuccessionPlanVariance item);
    bool Remove(string code);
    List<DefaultSuccessionPlanVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultSuccessionPlanVarianceStore(Dictionary<string, DefaultSuccessionPlanVariance> items, List<string> order) : IDefaultSuccessionPlanVarianceStore
{
    public Dictionary<string, DefaultSuccessionPlanVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultSuccessionPlanVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultSuccessionPlanVariance item)
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

    public List<DefaultSuccessionPlanVariance> List()
    {
        var outValue = new List<DefaultSuccessionPlanVariance>();
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
public sealed class CachedDefaultSuccessionPlanVarianceStore(MemoryDefaultSuccessionPlanVarianceStore inner, DefaultSuccessionPlanVarianceCache cache, int hits, int misses) : IDefaultSuccessionPlanVarianceStore
{
    public MemoryDefaultSuccessionPlanVarianceStore Inner { get; set; } = inner;
    public DefaultSuccessionPlanVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultSuccessionPlanVariance? Find(string code)
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

    public void Save(DefaultSuccessionPlanVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultSuccessionPlanVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default succession plan variances. Empty fields match all.</summary>
public sealed record DefaultSuccessionPlanVarianceQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultSuccessionPlanVariance item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultSuccessionPlanVariance> Apply(List<DefaultSuccessionPlanVariance> items)
    {
        var matched = Collections.Filter(items, (DefaultSuccessionPlanVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default succession plan variance store.</summary>
public static class DefaultSuccessionPlanVarianceStore
{
    public static IDefaultSuccessionPlanVarianceStore NewMemoryDefaultSuccessionPlanVarianceStore()
    {
        return new MemoryDefaultSuccessionPlanVarianceStore(
            new Dictionary<string, DefaultSuccessionPlanVariance>(),
            new List<string>());
    }

    public static IDefaultSuccessionPlanVarianceStore NewCachedDefaultSuccessionPlanVarianceStore(MemoryDefaultSuccessionPlanVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultSuccessionPlanVarianceStore(
            inner,
            DefaultSuccessionPlanVarianceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultSuccessionPlanVariance> FindDefaultSuccessionPlanVariances(IDefaultSuccessionPlanVarianceStore store, DefaultSuccessionPlanVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
