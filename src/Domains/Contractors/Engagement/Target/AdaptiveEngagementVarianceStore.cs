namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive engagement variances by code.</summary>
public interface IAdaptiveEngagementVarianceStore
{
    AdaptiveEngagementVariance? Find(string code);
    void Save(AdaptiveEngagementVariance item);
    bool Remove(string code);
    List<AdaptiveEngagementVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveEngagementVarianceStore(Dictionary<string, AdaptiveEngagementVariance> items, List<string> order) : IAdaptiveEngagementVarianceStore
{
    public Dictionary<string, AdaptiveEngagementVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveEngagementVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveEngagementVariance item)
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

    public List<AdaptiveEngagementVariance> List()
    {
        var outValue = new List<AdaptiveEngagementVariance>();
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
public sealed class CachedAdaptiveEngagementVarianceStore(MemoryAdaptiveEngagementVarianceStore inner, AdaptiveEngagementVarianceCache cache, int hits, int misses) : IAdaptiveEngagementVarianceStore
{
    public MemoryAdaptiveEngagementVarianceStore Inner { get; set; } = inner;
    public AdaptiveEngagementVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveEngagementVariance? Find(string code)
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

    public void Save(AdaptiveEngagementVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveEngagementVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive engagement variances. Empty fields match all.</summary>
public sealed record AdaptiveEngagementVarianceQuery(string Segment, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveEngagementVariance item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveEngagementVariance> Apply(List<AdaptiveEngagementVariance> items)
    {
        var matched = Collections.Filter(items, (AdaptiveEngagementVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive engagement variance store.</summary>
public static class AdaptiveEngagementVarianceStore
{
    public static IAdaptiveEngagementVarianceStore NewMemoryAdaptiveEngagementVarianceStore()
    {
        return new MemoryAdaptiveEngagementVarianceStore(
            new Dictionary<string, AdaptiveEngagementVariance>(),
            new List<string>());
    }

    public static IAdaptiveEngagementVarianceStore NewCachedAdaptiveEngagementVarianceStore(MemoryAdaptiveEngagementVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveEngagementVarianceStore(
            inner,
            AdaptiveEngagementVarianceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveEngagementVariance> FindAdaptiveEngagementVariances(IAdaptiveEngagementVarianceStore store, AdaptiveEngagementVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
