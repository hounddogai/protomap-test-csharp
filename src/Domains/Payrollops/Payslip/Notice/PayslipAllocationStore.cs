namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip allocations by code.</summary>
public interface IPayslipAllocationStore
{
    PayslipAllocation? Find(string code);
    void Save(PayslipAllocation item);
    bool Remove(string code);
    List<PayslipAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipAllocationStore(Dictionary<string, PayslipAllocation> items, List<string> order) : IPayslipAllocationStore
{
    public Dictionary<string, PayslipAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipAllocation item)
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

    public List<PayslipAllocation> List()
    {
        var outValue = new List<PayslipAllocation>();
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
public sealed class CachedPayslipAllocationStore(MemoryPayslipAllocationStore inner, PayslipAllocationCache cache, int hits, int misses) : IPayslipAllocationStore
{
    public MemoryPayslipAllocationStore Inner { get; set; } = inner;
    public PayslipAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipAllocation? Find(string code)
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

    public void Save(PayslipAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip allocations. Empty fields match all.</summary>
public sealed record PayslipAllocationQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipAllocation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipAllocation> Apply(List<PayslipAllocation> items)
    {
        var matched = Collections.Filter(items, (PayslipAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip allocation store.</summary>
public static class PayslipAllocationStore
{
    public static IPayslipAllocationStore NewMemoryPayslipAllocationStore()
    {
        return new MemoryPayslipAllocationStore(new Dictionary<string, PayslipAllocation>(), new List<string>());
    }

    public static IPayslipAllocationStore NewCachedPayslipAllocationStore(MemoryPayslipAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipAllocationStore(inner, PayslipAllocationCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayslipAllocation> FindPayslipAllocations(IPayslipAllocationStore store, PayslipAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
