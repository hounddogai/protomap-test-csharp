namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount variances by code.</summary>
public interface IHeadcountVarianceStore
{
    HeadcountVariance? Find(string code);
    void Save(HeadcountVariance item);
    bool Remove(string code);
    List<HeadcountVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountVarianceStore(Dictionary<string, HeadcountVariance> items, List<string> order) : IHeadcountVarianceStore
{
    public Dictionary<string, HeadcountVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountVariance item)
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

    public List<HeadcountVariance> List()
    {
        var outValue = new List<HeadcountVariance>();
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
public sealed class CachedHeadcountVarianceStore(MemoryHeadcountVarianceStore inner, HeadcountVarianceCache cache, int hits, int misses) : IHeadcountVarianceStore
{
    public MemoryHeadcountVarianceStore Inner { get; set; } = inner;
    public HeadcountVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountVariance? Find(string code)
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

    public void Save(HeadcountVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount variances. Empty fields match all.</summary>
public sealed record HeadcountVarianceQuery(string Unit, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountVariance item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountVariance> Apply(List<HeadcountVariance> items)
    {
        var matched = Collections.Filter(items, (HeadcountVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount variance store.</summary>
public static class HeadcountVarianceStore
{
    public static IHeadcountVarianceStore NewMemoryHeadcountVarianceStore()
    {
        return new MemoryHeadcountVarianceStore(new Dictionary<string, HeadcountVariance>(), new List<string>());
    }

    public static IHeadcountVarianceStore NewCachedHeadcountVarianceStore(MemoryHeadcountVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountVarianceStore(inner, HeadcountVarianceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<HeadcountVariance> FindHeadcountVariances(IHeadcountVarianceStore store, HeadcountVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
