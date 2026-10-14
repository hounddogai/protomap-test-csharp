namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive holiday variances by code.</summary>
public interface IAdaptiveHolidayVarianceStore
{
    AdaptiveHolidayVariance? Find(string code);
    void Save(AdaptiveHolidayVariance item);
    bool Remove(string code);
    List<AdaptiveHolidayVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveHolidayVarianceStore(Dictionary<string, AdaptiveHolidayVariance> items, List<string> order) : IAdaptiveHolidayVarianceStore
{
    public Dictionary<string, AdaptiveHolidayVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveHolidayVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveHolidayVariance item)
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

    public List<AdaptiveHolidayVariance> List()
    {
        var outValue = new List<AdaptiveHolidayVariance>();
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
public sealed class CachedAdaptiveHolidayVarianceStore(MemoryAdaptiveHolidayVarianceStore inner, AdaptiveHolidayVarianceCache cache, int hits, int misses) : IAdaptiveHolidayVarianceStore
{
    public MemoryAdaptiveHolidayVarianceStore Inner { get; set; } = inner;
    public AdaptiveHolidayVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveHolidayVariance? Find(string code)
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

    public void Save(AdaptiveHolidayVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveHolidayVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive holiday variances. Empty fields match all.</summary>
public sealed record AdaptiveHolidayVarianceQuery(string Label, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveHolidayVariance item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveHolidayVariance> Apply(List<AdaptiveHolidayVariance> items)
    {
        var matched = Collections.Filter(items, (AdaptiveHolidayVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive holiday variance store.</summary>
public static class AdaptiveHolidayVarianceStore
{
    public static IAdaptiveHolidayVarianceStore NewMemoryAdaptiveHolidayVarianceStore()
    {
        return new MemoryAdaptiveHolidayVarianceStore(
            new Dictionary<string, AdaptiveHolidayVariance>(),
            new List<string>());
    }

    public static IAdaptiveHolidayVarianceStore NewCachedAdaptiveHolidayVarianceStore(MemoryAdaptiveHolidayVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveHolidayVarianceStore(
            inner,
            AdaptiveHolidayVarianceCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveHolidayVariance> FindAdaptiveHolidayVariances(IAdaptiveHolidayVarianceStore store, AdaptiveHolidayVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
