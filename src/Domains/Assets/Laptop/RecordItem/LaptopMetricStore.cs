namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop metrics by code.</summary>
public interface ILaptopMetricStore
{
    LaptopMetric? Find(string code);
    void Save(LaptopMetric item);
    bool Remove(string code);
    List<LaptopMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopMetricStore(Dictionary<string, LaptopMetric> items, List<string> order) : ILaptopMetricStore
{
    public Dictionary<string, LaptopMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopMetric item)
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

    public List<LaptopMetric> List()
    {
        var outValue = new List<LaptopMetric>();
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
public sealed class CachedLaptopMetricStore(MemoryLaptopMetricStore inner, LaptopMetricCache cache, int hits, int misses) : ILaptopMetricStore
{
    public MemoryLaptopMetricStore Inner { get; set; } = inner;
    public LaptopMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopMetric? Find(string code)
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

    public void Save(LaptopMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop metrics. Empty fields match all.</summary>
public sealed record LaptopMetricQuery(string Department, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopMetric item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopMetric> Apply(List<LaptopMetric> items)
    {
        var matched = Collections.Filter(items, (LaptopMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop metric store.</summary>
public static class LaptopMetricStore
{
    public static ILaptopMetricStore NewMemoryLaptopMetricStore()
    {
        return new MemoryLaptopMetricStore(new Dictionary<string, LaptopMetric>(), new List<string>());
    }

    public static ILaptopMetricStore NewCachedLaptopMetricStore(MemoryLaptopMetricStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopMetricStore(inner, LaptopMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LaptopMetric> FindLaptopMetrics(ILaptopMetricStore store, LaptopMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
