namespace Acme.Workforce.Contractors.Vendor.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor metrics by code.</summary>
public interface IVendorMetricStore
{
    VendorMetric? Find(string code);
    void Save(VendorMetric item);
    bool Remove(string code);
    List<VendorMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorMetricStore(Dictionary<string, VendorMetric> items, List<string> order) : IVendorMetricStore
{
    public Dictionary<string, VendorMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorMetric item)
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

    public List<VendorMetric> List()
    {
        var outValue = new List<VendorMetric>();
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
public sealed class CachedVendorMetricStore(MemoryVendorMetricStore inner, VendorMetricCache cache, int hits, int misses) : IVendorMetricStore
{
    public MemoryVendorMetricStore Inner { get; set; } = inner;
    public VendorMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorMetric? Find(string code)
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

    public void Save(VendorMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor metrics. Empty fields match all.</summary>
public sealed record VendorMetricQuery(string Label, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(VendorMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorMetric> Apply(List<VendorMetric> items)
    {
        var matched = Collections.Filter(items, (VendorMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor metric store.</summary>
public static class VendorMetricStore
{
    public static IVendorMetricStore NewMemoryVendorMetricStore()
    {
        return new MemoryVendorMetricStore(new Dictionary<string, VendorMetric>(), new List<string>());
    }

    public static IVendorMetricStore NewCachedVendorMetricStore(MemoryVendorMetricStore inner, int ttl, IClock clock)
    {
        return new CachedVendorMetricStore(inner, VendorMetricCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<VendorMetric> FindVendorMetrics(IVendorMetricStore store, VendorMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
