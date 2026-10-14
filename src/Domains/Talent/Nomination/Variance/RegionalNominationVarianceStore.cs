namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional nomination variances by code.</summary>
public interface IRegionalNominationVarianceStore
{
    RegionalNominationVariance? Find(string code);
    void Save(RegionalNominationVariance item);
    bool Remove(string code);
    List<RegionalNominationVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalNominationVarianceStore(Dictionary<string, RegionalNominationVariance> items, List<string> order) : IRegionalNominationVarianceStore
{
    public Dictionary<string, RegionalNominationVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalNominationVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalNominationVariance item)
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

    public List<RegionalNominationVariance> List()
    {
        var outValue = new List<RegionalNominationVariance>();
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
public sealed class CachedRegionalNominationVarianceStore(MemoryRegionalNominationVarianceStore inner, RegionalNominationVarianceCache cache, int hits, int misses) : IRegionalNominationVarianceStore
{
    public MemoryRegionalNominationVarianceStore Inner { get; set; } = inner;
    public RegionalNominationVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalNominationVariance? Find(string code)
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

    public void Save(RegionalNominationVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalNominationVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional nomination variances. Empty fields match all.</summary>
public sealed record RegionalNominationVarianceQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalNominationVariance item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalNominationVariance> Apply(List<RegionalNominationVariance> items)
    {
        var matched = Collections.Filter(items, (RegionalNominationVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional nomination variance store.</summary>
public static class RegionalNominationVarianceStore
{
    public static IRegionalNominationVarianceStore NewMemoryRegionalNominationVarianceStore()
    {
        return new MemoryRegionalNominationVarianceStore(
            new Dictionary<string, RegionalNominationVariance>(),
            new List<string>());
    }

    public static IRegionalNominationVarianceStore NewCachedRegionalNominationVarianceStore(MemoryRegionalNominationVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalNominationVarianceStore(
            inner,
            RegionalNominationVarianceCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalNominationVariance> FindRegionalNominationVariances(IRegionalNominationVarianceStore store, RegionalNominationVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
