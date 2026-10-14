namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered desk metrics by code.</summary>
public interface ITieredDeskMetricStore
{
    TieredDeskMetric? Find(string code);
    void Save(TieredDeskMetric item);
    bool Remove(string code);
    List<TieredDeskMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredDeskMetricStore(Dictionary<string, TieredDeskMetric> items, List<string> order) : ITieredDeskMetricStore
{
    public Dictionary<string, TieredDeskMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredDeskMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredDeskMetric item)
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

    public List<TieredDeskMetric> List()
    {
        var outValue = new List<TieredDeskMetric>();
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
public sealed class CachedTieredDeskMetricStore(MemoryTieredDeskMetricStore inner, TieredDeskMetricCache cache, int hits, int misses) : ITieredDeskMetricStore
{
    public MemoryTieredDeskMetricStore Inner { get; set; } = inner;
    public TieredDeskMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredDeskMetric? Find(string code)
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

    public void Save(TieredDeskMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredDeskMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered desk metrics. Empty fields match all.</summary>
public sealed record TieredDeskMetricQuery(string Segment, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(TieredDeskMetric item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredDeskMetric> Apply(List<TieredDeskMetric> items)
    {
        var matched = Collections.Filter(items, (TieredDeskMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered desk metric store.</summary>
public static class TieredDeskMetricStore
{
    public static ITieredDeskMetricStore NewMemoryTieredDeskMetricStore()
    {
        return new MemoryTieredDeskMetricStore(new Dictionary<string, TieredDeskMetric>(), new List<string>());
    }

    public static ITieredDeskMetricStore NewCachedTieredDeskMetricStore(MemoryTieredDeskMetricStore inner, int ttl, IClock clock)
    {
        return new CachedTieredDeskMetricStore(inner, TieredDeskMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TieredDeskMetric> FindTieredDeskMetrics(ITieredDeskMetricStore store, TieredDeskMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
