namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility variances by code.</summary>
public interface IMobilityVarianceStore
{
    MobilityVariance? Find(string code);
    void Save(MobilityVariance item);
    bool Remove(string code);
    List<MobilityVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityVarianceStore(Dictionary<string, MobilityVariance> items, List<string> order) : IMobilityVarianceStore
{
    public Dictionary<string, MobilityVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityVariance item)
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

    public List<MobilityVariance> List()
    {
        var outValue = new List<MobilityVariance>();
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
public sealed class CachedMobilityVarianceStore(MemoryMobilityVarianceStore inner, MobilityVarianceCache cache, int hits, int misses) : IMobilityVarianceStore
{
    public MemoryMobilityVarianceStore Inner { get; set; } = inner;
    public MobilityVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityVariance? Find(string code)
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

    public void Save(MobilityVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility variances. Empty fields match all.</summary>
public sealed record MobilityVarianceQuery(string Team, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityVariance item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityVariance> Apply(List<MobilityVariance> items)
    {
        var matched = Collections.Filter(items, (MobilityVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility variance store.</summary>
public static class MobilityVarianceStore
{
    public static IMobilityVarianceStore NewMemoryMobilityVarianceStore()
    {
        return new MemoryMobilityVarianceStore(new Dictionary<string, MobilityVariance>(), new List<string>());
    }

    public static IMobilityVarianceStore NewCachedMobilityVarianceStore(MemoryMobilityVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityVarianceStore(inner, MobilityVarianceCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<MobilityVariance> FindMobilityVariances(IMobilityVarianceStore store, MobilityVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
