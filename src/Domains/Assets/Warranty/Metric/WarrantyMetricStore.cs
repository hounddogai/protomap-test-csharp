namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty metrics by code.</summary>
public interface IWarrantyMetricStore
{
    WarrantyMetric? Find(string code);
    void Save(WarrantyMetric item);
    bool Remove(string code);
    List<WarrantyMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyMetricStore(Dictionary<string, WarrantyMetric> items, List<string> order) : IWarrantyMetricStore
{
    public Dictionary<string, WarrantyMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyMetric item)
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

    public List<WarrantyMetric> List()
    {
        var outValue = new List<WarrantyMetric>();
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
public sealed class CachedWarrantyMetricStore(MemoryWarrantyMetricStore inner, WarrantyMetricCache cache, int hits, int misses) : IWarrantyMetricStore
{
    public MemoryWarrantyMetricStore Inner { get; set; } = inner;
    public WarrantyMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyMetric? Find(string code)
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

    public void Save(WarrantyMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty metrics. Empty fields match all.</summary>
public sealed record WarrantyMetricQuery(string Region, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyMetric item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyMetric> Apply(List<WarrantyMetric> items)
    {
        var matched = Collections.Filter(items, (WarrantyMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty metric store.</summary>
public static class WarrantyMetricStore
{
    public static IWarrantyMetricStore NewMemoryWarrantyMetricStore()
    {
        return new MemoryWarrantyMetricStore(new Dictionary<string, WarrantyMetric>(), new List<string>());
    }

    public static IWarrantyMetricStore NewCachedWarrantyMetricStore(MemoryWarrantyMetricStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyMetricStore(inner, WarrantyMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<WarrantyMetric> FindWarrantyMetrics(IWarrantyMetricStore store, WarrantyMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
