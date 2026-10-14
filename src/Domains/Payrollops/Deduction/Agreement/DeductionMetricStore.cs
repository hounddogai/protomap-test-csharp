namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction metrics by code.</summary>
public interface IDeductionMetricStore
{
    DeductionMetric? Find(string code);
    void Save(DeductionMetric item);
    bool Remove(string code);
    List<DeductionMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionMetricStore(Dictionary<string, DeductionMetric> items, List<string> order) : IDeductionMetricStore
{
    public Dictionary<string, DeductionMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionMetric item)
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

    public List<DeductionMetric> List()
    {
        var outValue = new List<DeductionMetric>();
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
public sealed class CachedDeductionMetricStore(MemoryDeductionMetricStore inner, DeductionMetricCache cache, int hits, int misses) : IDeductionMetricStore
{
    public MemoryDeductionMetricStore Inner { get; set; } = inner;
    public DeductionMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionMetric? Find(string code)
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

    public void Save(DeductionMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction metrics. Empty fields match all.</summary>
public sealed record DeductionMetricQuery(string Source, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionMetric item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<DeductionMetric> Apply(List<DeductionMetric> items)
    {
        var matched = Collections.Filter(items, (DeductionMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction metric store.</summary>
public static class DeductionMetricStore
{
    public static IDeductionMetricStore NewMemoryDeductionMetricStore()
    {
        return new MemoryDeductionMetricStore(new Dictionary<string, DeductionMetric>(), new List<string>());
    }

    public static IDeductionMetricStore NewCachedDeductionMetricStore(MemoryDeductionMetricStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionMetricStore(inner, DeductionMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DeductionMetric> FindDeductionMetrics(IDeductionMetricStore store, DeductionMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
