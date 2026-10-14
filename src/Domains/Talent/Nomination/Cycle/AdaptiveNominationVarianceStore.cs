namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive nomination variances by code.</summary>
public interface IAdaptiveNominationVarianceStore
{
    AdaptiveNominationVariance? Find(string code);
    void Save(AdaptiveNominationVariance item);
    bool Remove(string code);
    List<AdaptiveNominationVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveNominationVarianceStore(Dictionary<string, AdaptiveNominationVariance> items, List<string> order) : IAdaptiveNominationVarianceStore
{
    public Dictionary<string, AdaptiveNominationVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveNominationVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveNominationVariance item)
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

    public List<AdaptiveNominationVariance> List()
    {
        var outValue = new List<AdaptiveNominationVariance>();
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
public sealed class CachedAdaptiveNominationVarianceStore(MemoryAdaptiveNominationVarianceStore inner, AdaptiveNominationVarianceCache cache, int hits, int misses) : IAdaptiveNominationVarianceStore
{
    public MemoryAdaptiveNominationVarianceStore Inner { get; set; } = inner;
    public AdaptiveNominationVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveNominationVariance? Find(string code)
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

    public void Save(AdaptiveNominationVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveNominationVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive nomination variances. Empty fields match all.</summary>
public sealed record AdaptiveNominationVarianceQuery(string Source, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveNominationVariance item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveNominationVariance> Apply(List<AdaptiveNominationVariance> items)
    {
        var matched = Collections.Filter(items, (AdaptiveNominationVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive nomination variance store.</summary>
public static class AdaptiveNominationVarianceStore
{
    public static IAdaptiveNominationVarianceStore NewMemoryAdaptiveNominationVarianceStore()
    {
        return new MemoryAdaptiveNominationVarianceStore(
            new Dictionary<string, AdaptiveNominationVariance>(),
            new List<string>());
    }

    public static IAdaptiveNominationVarianceStore NewCachedAdaptiveNominationVarianceStore(MemoryAdaptiveNominationVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveNominationVarianceStore(
            inner,
            AdaptiveNominationVarianceCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveNominationVariance> FindAdaptiveNominationVariances(IAdaptiveNominationVarianceStore store, AdaptiveNominationVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
