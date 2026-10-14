namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount metrics by code.</summary>
public interface IHeadcountMetricStore
{
    HeadcountMetric? Find(string code);
    void Save(HeadcountMetric item);
    bool Remove(string code);
    List<HeadcountMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountMetricStore(Dictionary<string, HeadcountMetric> items, List<string> order) : IHeadcountMetricStore
{
    public Dictionary<string, HeadcountMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountMetric item)
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

    public List<HeadcountMetric> List()
    {
        var outValue = new List<HeadcountMetric>();
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
public sealed class CachedHeadcountMetricStore(MemoryHeadcountMetricStore inner, HeadcountMetricCache cache, int hits, int misses) : IHeadcountMetricStore
{
    public MemoryHeadcountMetricStore Inner { get; set; } = inner;
    public HeadcountMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountMetric? Find(string code)
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

    public void Save(HeadcountMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount metrics. Empty fields match all.</summary>
public sealed record HeadcountMetricQuery(string Channel, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountMetric item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountMetric> Apply(List<HeadcountMetric> items)
    {
        var matched = Collections.Filter(items, (HeadcountMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount metric store.</summary>
public static class HeadcountMetricStore
{
    public static IHeadcountMetricStore NewMemoryHeadcountMetricStore()
    {
        return new MemoryHeadcountMetricStore(new Dictionary<string, HeadcountMetric>(), new List<string>());
    }

    public static IHeadcountMetricStore NewCachedHeadcountMetricStore(MemoryHeadcountMetricStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountMetricStore(inner, HeadcountMetricCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<HeadcountMetric> FindHeadcountMetrics(IHeadcountMetricStore store, HeadcountMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
