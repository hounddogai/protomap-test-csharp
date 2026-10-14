namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled payslip allocations by code.</summary>
public interface IScheduledPayslipAllocationStore
{
    ScheduledPayslipAllocation? Find(string code);
    void Save(ScheduledPayslipAllocation item);
    bool Remove(string code);
    List<ScheduledPayslipAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledPayslipAllocationStore(Dictionary<string, ScheduledPayslipAllocation> items, List<string> order) : IScheduledPayslipAllocationStore
{
    public Dictionary<string, ScheduledPayslipAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledPayslipAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledPayslipAllocation item)
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

    public List<ScheduledPayslipAllocation> List()
    {
        var outValue = new List<ScheduledPayslipAllocation>();
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
public sealed class CachedScheduledPayslipAllocationStore(MemoryScheduledPayslipAllocationStore inner, ScheduledPayslipAllocationCache cache, int hits, int misses) : IScheduledPayslipAllocationStore
{
    public MemoryScheduledPayslipAllocationStore Inner { get; set; } = inner;
    public ScheduledPayslipAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledPayslipAllocation? Find(string code)
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

    public void Save(ScheduledPayslipAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledPayslipAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled payslip allocations. Empty fields match all.</summary>
public sealed record ScheduledPayslipAllocationQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledPayslipAllocation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledPayslipAllocation> Apply(List<ScheduledPayslipAllocation> items)
    {
        var matched = Collections.Filter(items, (ScheduledPayslipAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled payslip allocation store.</summary>
public static class ScheduledPayslipAllocationStore
{
    public static IScheduledPayslipAllocationStore NewMemoryScheduledPayslipAllocationStore()
    {
        return new MemoryScheduledPayslipAllocationStore(
            new Dictionary<string, ScheduledPayslipAllocation>(),
            new List<string>());
    }

    public static IScheduledPayslipAllocationStore NewCachedScheduledPayslipAllocationStore(MemoryScheduledPayslipAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledPayslipAllocationStore(
            inner,
            ScheduledPayslipAllocationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledPayslipAllocation> FindScheduledPayslipAllocations(IScheduledPayslipAllocationStore store, ScheduledPayslipAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
