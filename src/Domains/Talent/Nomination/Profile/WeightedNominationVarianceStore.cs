namespace Acme.Workforce.Talent.Nomination.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted nomination variances by code.</summary>
public interface IWeightedNominationVarianceStore
{
    WeightedNominationVariance? Find(string code);
    void Save(WeightedNominationVariance item);
    bool Remove(string code);
    List<WeightedNominationVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedNominationVarianceStore(Dictionary<string, WeightedNominationVariance> items, List<string> order) : IWeightedNominationVarianceStore
{
    public Dictionary<string, WeightedNominationVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedNominationVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedNominationVariance item)
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

    public List<WeightedNominationVariance> List()
    {
        var outValue = new List<WeightedNominationVariance>();
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
public sealed class CachedWeightedNominationVarianceStore(MemoryWeightedNominationVarianceStore inner, WeightedNominationVarianceCache cache, int hits, int misses) : IWeightedNominationVarianceStore
{
    public MemoryWeightedNominationVarianceStore Inner { get; set; } = inner;
    public WeightedNominationVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedNominationVariance? Find(string code)
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

    public void Save(WeightedNominationVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedNominationVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted nomination variances. Empty fields match all.</summary>
public sealed record WeightedNominationVarianceQuery(string Team, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedNominationVariance item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedNominationVariance> Apply(List<WeightedNominationVariance> items)
    {
        var matched = Collections.Filter(items, (WeightedNominationVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted nomination variance store.</summary>
public static class WeightedNominationVarianceStore
{
    public static IWeightedNominationVarianceStore NewMemoryWeightedNominationVarianceStore()
    {
        return new MemoryWeightedNominationVarianceStore(
            new Dictionary<string, WeightedNominationVariance>(),
            new List<string>());
    }

    public static IWeightedNominationVarianceStore NewCachedWeightedNominationVarianceStore(MemoryWeightedNominationVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedNominationVarianceStore(
            inner,
            WeightedNominationVarianceCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedNominationVariance> FindWeightedNominationVariances(IWeightedNominationVarianceStore store, WeightedNominationVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
