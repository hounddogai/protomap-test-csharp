namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle metrics by code.</summary>
public interface IMeritCycleMetricStore
{
    MeritCycleMetric? Find(string code);
    void Save(MeritCycleMetric item);
    bool Remove(string code);
    List<MeritCycleMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleMetricStore(Dictionary<string, MeritCycleMetric> items, List<string> order) : IMeritCycleMetricStore
{
    public Dictionary<string, MeritCycleMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleMetric item)
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

    public List<MeritCycleMetric> List()
    {
        var outValue = new List<MeritCycleMetric>();
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
public sealed class CachedMeritCycleMetricStore(MemoryMeritCycleMetricStore inner, MeritCycleMetricCache cache, int hits, int misses) : IMeritCycleMetricStore
{
    public MemoryMeritCycleMetricStore Inner { get; set; } = inner;
    public MeritCycleMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleMetric? Find(string code)
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

    public void Save(MeritCycleMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle metrics. Empty fields match all.</summary>
public sealed record MeritCycleMetricQuery(string Country, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleMetric item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<MeritCycleMetric> Apply(List<MeritCycleMetric> items)
    {
        var matched = Collections.Filter(items, (MeritCycleMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle metric store.</summary>
public static class MeritCycleMetricStore
{
    public static IMeritCycleMetricStore NewMemoryMeritCycleMetricStore()
    {
        return new MemoryMeritCycleMetricStore(new Dictionary<string, MeritCycleMetric>(), new List<string>());
    }

    public static IMeritCycleMetricStore NewCachedMeritCycleMetricStore(MemoryMeritCycleMetricStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleMetricStore(inner, MeritCycleMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<MeritCycleMetric> FindMeritCycleMetrics(IMeritCycleMetricStore store, MeritCycleMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
