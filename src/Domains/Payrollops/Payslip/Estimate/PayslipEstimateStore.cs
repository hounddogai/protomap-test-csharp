namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip estimates by code.</summary>
public interface IPayslipEstimateStore
{
    PayslipEstimate? Find(string code);
    void Save(PayslipEstimate item);
    bool Remove(string code);
    List<PayslipEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipEstimateStore(Dictionary<string, PayslipEstimate> items, List<string> order) : IPayslipEstimateStore
{
    public Dictionary<string, PayslipEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipEstimate item)
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

    public List<PayslipEstimate> List()
    {
        var outValue = new List<PayslipEstimate>();
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
public sealed class CachedPayslipEstimateStore(MemoryPayslipEstimateStore inner, PayslipEstimateCache cache, int hits, int misses) : IPayslipEstimateStore
{
    public MemoryPayslipEstimateStore Inner { get; set; } = inner;
    public PayslipEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipEstimate? Find(string code)
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

    public void Save(PayslipEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip estimates. Empty fields match all.</summary>
public sealed record PayslipEstimateQuery(string Channel, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipEstimate item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipEstimate> Apply(List<PayslipEstimate> items)
    {
        var matched = Collections.Filter(items, (PayslipEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip estimate store.</summary>
public static class PayslipEstimateStore
{
    public static IPayslipEstimateStore NewMemoryPayslipEstimateStore()
    {
        return new MemoryPayslipEstimateStore(new Dictionary<string, PayslipEstimate>(), new List<string>());
    }

    public static IPayslipEstimateStore NewCachedPayslipEstimateStore(MemoryPayslipEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipEstimateStore(inner, PayslipEstimateCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayslipEstimate> FindPayslipEstimates(IPayslipEstimateStore store, PayslipEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
