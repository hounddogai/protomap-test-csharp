namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual balance metrics by code.</summary>
public interface IManualBalanceMetricStore
{
    ManualBalanceMetric? Find(string code);
    void Save(ManualBalanceMetric item);
    bool Remove(string code);
    List<ManualBalanceMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualBalanceMetricStore(Dictionary<string, ManualBalanceMetric> items, List<string> order) : IManualBalanceMetricStore
{
    public Dictionary<string, ManualBalanceMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualBalanceMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualBalanceMetric item)
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

    public List<ManualBalanceMetric> List()
    {
        var outValue = new List<ManualBalanceMetric>();
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
public sealed class CachedManualBalanceMetricStore(MemoryManualBalanceMetricStore inner, ManualBalanceMetricCache cache, int hits, int misses) : IManualBalanceMetricStore
{
    public MemoryManualBalanceMetricStore Inner { get; set; } = inner;
    public ManualBalanceMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualBalanceMetric? Find(string code)
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

    public void Save(ManualBalanceMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualBalanceMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual balance metrics. Empty fields match all.</summary>
public sealed record ManualBalanceMetricQuery(string Segment, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualBalanceMetric item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualBalanceMetric> Apply(List<ManualBalanceMetric> items)
    {
        var matched = Collections.Filter(items, (ManualBalanceMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual balance metric store.</summary>
public static class ManualBalanceMetricStore
{
    public static IManualBalanceMetricStore NewMemoryManualBalanceMetricStore()
    {
        return new MemoryManualBalanceMetricStore(new Dictionary<string, ManualBalanceMetric>(), new List<string>());
    }

    public static IManualBalanceMetricStore NewCachedManualBalanceMetricStore(MemoryManualBalanceMetricStore inner, int ttl, IClock clock)
    {
        return new CachedManualBalanceMetricStore(inner, ManualBalanceMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ManualBalanceMetric> FindManualBalanceMetrics(IManualBalanceMetricStore store, ManualBalanceMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
