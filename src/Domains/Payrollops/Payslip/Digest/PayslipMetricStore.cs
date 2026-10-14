namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip metrics by code.</summary>
public interface IPayslipMetricStore
{
    PayslipMetric? Find(string code);
    void Save(PayslipMetric item);
    bool Remove(string code);
    List<PayslipMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipMetricStore(Dictionary<string, PayslipMetric> items, List<string> order) : IPayslipMetricStore
{
    public Dictionary<string, PayslipMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipMetric item)
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

    public List<PayslipMetric> List()
    {
        var outValue = new List<PayslipMetric>();
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
public sealed class CachedPayslipMetricStore(MemoryPayslipMetricStore inner, PayslipMetricCache cache, int hits, int misses) : IPayslipMetricStore
{
    public MemoryPayslipMetricStore Inner { get; set; } = inner;
    public PayslipMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipMetric? Find(string code)
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

    public void Save(PayslipMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip metrics. Empty fields match all.</summary>
public sealed record PayslipMetricQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipMetric item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<PayslipMetric> Apply(List<PayslipMetric> items)
    {
        var matched = Collections.Filter(items, (PayslipMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip metric store.</summary>
public static class PayslipMetricStore
{
    public static IPayslipMetricStore NewMemoryPayslipMetricStore()
    {
        return new MemoryPayslipMetricStore(new Dictionary<string, PayslipMetric>(), new List<string>());
    }

    public static IPayslipMetricStore NewCachedPayslipMetricStore(MemoryPayslipMetricStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipMetricStore(inner, PayslipMetricCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayslipMetric> FindPayslipMetrics(IPayslipMetricStore store, PayslipMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
