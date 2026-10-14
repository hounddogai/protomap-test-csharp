namespace Acme.Workforce.Contractors.Rate.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate metrics by code.</summary>
public interface IRateMetricStore
{
    RateMetric? Find(string code);
    void Save(RateMetric item);
    bool Remove(string code);
    List<RateMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateMetricStore(Dictionary<string, RateMetric> items, List<string> order) : IRateMetricStore
{
    public Dictionary<string, RateMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateMetric item)
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

    public List<RateMetric> List()
    {
        var outValue = new List<RateMetric>();
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
public sealed class CachedRateMetricStore(MemoryRateMetricStore inner, RateMetricCache cache, int hits, int misses) : IRateMetricStore
{
    public MemoryRateMetricStore Inner { get; set; } = inner;
    public RateMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateMetric? Find(string code)
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

    public void Save(RateMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate metrics. Empty fields match all.</summary>
public sealed record RateMetricQuery(string Channel, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(RateMetric item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateMetric> Apply(List<RateMetric> items)
    {
        var matched = Collections.Filter(items, (RateMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate metric store.</summary>
public static class RateMetricStore
{
    public static IRateMetricStore NewMemoryRateMetricStore()
    {
        return new MemoryRateMetricStore(new Dictionary<string, RateMetric>(), new List<string>());
    }

    public static IRateMetricStore NewCachedRateMetricStore(MemoryRateMetricStore inner, int ttl, IClock clock)
    {
        return new CachedRateMetricStore(inner, RateMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RateMetric> FindRateMetrics(IRateMetricStore store, RateMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
