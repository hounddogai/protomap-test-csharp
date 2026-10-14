namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual payslip allocations by code.</summary>
public interface IManualPayslipAllocationStore
{
    ManualPayslipAllocation? Find(string code);
    void Save(ManualPayslipAllocation item);
    bool Remove(string code);
    List<ManualPayslipAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPayslipAllocationStore(Dictionary<string, ManualPayslipAllocation> items, List<string> order) : IManualPayslipAllocationStore
{
    public Dictionary<string, ManualPayslipAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPayslipAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPayslipAllocation item)
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

    public List<ManualPayslipAllocation> List()
    {
        var outValue = new List<ManualPayslipAllocation>();
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
public sealed class CachedManualPayslipAllocationStore(MemoryManualPayslipAllocationStore inner, ManualPayslipAllocationCache cache, int hits, int misses) : IManualPayslipAllocationStore
{
    public MemoryManualPayslipAllocationStore Inner { get; set; } = inner;
    public ManualPayslipAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPayslipAllocation? Find(string code)
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

    public void Save(ManualPayslipAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPayslipAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual payslip allocations. Empty fields match all.</summary>
public sealed record ManualPayslipAllocationQuery(string Country, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPayslipAllocation item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualPayslipAllocation> Apply(List<ManualPayslipAllocation> items)
    {
        var matched = Collections.Filter(items, (ManualPayslipAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual payslip allocation store.</summary>
public static class ManualPayslipAllocationStore
{
    public static IManualPayslipAllocationStore NewMemoryManualPayslipAllocationStore()
    {
        return new MemoryManualPayslipAllocationStore(
            new Dictionary<string, ManualPayslipAllocation>(),
            new List<string>());
    }

    public static IManualPayslipAllocationStore NewCachedManualPayslipAllocationStore(MemoryManualPayslipAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedManualPayslipAllocationStore(
            inner,
            ManualPayslipAllocationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ManualPayslipAllocation> FindManualPayslipAllocations(IManualPayslipAllocationStore store, ManualPayslipAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
