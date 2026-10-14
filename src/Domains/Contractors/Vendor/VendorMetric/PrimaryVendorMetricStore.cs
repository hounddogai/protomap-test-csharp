namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary vendor metrics by code.</summary>
public interface IPrimaryVendorMetricStore
{
    PrimaryVendorMetric? Find(string code);
    void Save(PrimaryVendorMetric item);
    bool Remove(string code);
    List<PrimaryVendorMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryVendorMetricStore(Dictionary<string, PrimaryVendorMetric> items, List<string> order) : IPrimaryVendorMetricStore
{
    public Dictionary<string, PrimaryVendorMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryVendorMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryVendorMetric item)
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

    public List<PrimaryVendorMetric> List()
    {
        var outValue = new List<PrimaryVendorMetric>();
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
public sealed class CachedPrimaryVendorMetricStore(MemoryPrimaryVendorMetricStore inner, PrimaryVendorMetricCache cache, int hits, int misses) : IPrimaryVendorMetricStore
{
    public MemoryPrimaryVendorMetricStore Inner { get; set; } = inner;
    public PrimaryVendorMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryVendorMetric? Find(string code)
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

    public void Save(PrimaryVendorMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryVendorMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary vendor metrics. Empty fields match all.</summary>
public sealed record PrimaryVendorMetricQuery(string Department, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryVendorMetric item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryVendorMetric> Apply(List<PrimaryVendorMetric> items)
    {
        var matched = Collections.Filter(items, (PrimaryVendorMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary vendor metric store.</summary>
public static class PrimaryVendorMetricStore
{
    public static IPrimaryVendorMetricStore NewMemoryPrimaryVendorMetricStore()
    {
        return new MemoryPrimaryVendorMetricStore(new Dictionary<string, PrimaryVendorMetric>(), new List<string>());
    }

    public static IPrimaryVendorMetricStore NewCachedPrimaryVendorMetricStore(MemoryPrimaryVendorMetricStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryVendorMetricStore(inner, PrimaryVendorMetricCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PrimaryVendorMetric> FindPrimaryVendorMetrics(IPrimaryVendorMetricStore store, PrimaryVendorMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
