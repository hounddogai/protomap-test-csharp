namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual payslip estimates by code.</summary>
public interface IManualPayslipEstimateStore
{
    ManualPayslipEstimate? Find(string code);
    void Save(ManualPayslipEstimate item);
    bool Remove(string code);
    List<ManualPayslipEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPayslipEstimateStore(Dictionary<string, ManualPayslipEstimate> items, List<string> order) : IManualPayslipEstimateStore
{
    public Dictionary<string, ManualPayslipEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPayslipEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPayslipEstimate item)
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

    public List<ManualPayslipEstimate> List()
    {
        var outValue = new List<ManualPayslipEstimate>();
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
public sealed class CachedManualPayslipEstimateStore(MemoryManualPayslipEstimateStore inner, ManualPayslipEstimateCache cache, int hits, int misses) : IManualPayslipEstimateStore
{
    public MemoryManualPayslipEstimateStore Inner { get; set; } = inner;
    public ManualPayslipEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPayslipEstimate? Find(string code)
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

    public void Save(ManualPayslipEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPayslipEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual payslip estimates. Empty fields match all.</summary>
public sealed record ManualPayslipEstimateQuery(string Department, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPayslipEstimate item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualPayslipEstimate> Apply(List<ManualPayslipEstimate> items)
    {
        var matched = Collections.Filter(items, (ManualPayslipEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual payslip estimate store.</summary>
public static class ManualPayslipEstimateStore
{
    public static IManualPayslipEstimateStore NewMemoryManualPayslipEstimateStore()
    {
        return new MemoryManualPayslipEstimateStore(
            new Dictionary<string, ManualPayslipEstimate>(),
            new List<string>());
    }

    public static IManualPayslipEstimateStore NewCachedManualPayslipEstimateStore(MemoryManualPayslipEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedManualPayslipEstimateStore(inner, ManualPayslipEstimateCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ManualPayslipEstimate> FindManualPayslipEstimates(IManualPayslipEstimateStore store, ManualPayslipEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
