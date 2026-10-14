namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip adjustments by code.</summary>
public interface IPayslipAdjustmentStore
{
    PayslipAdjustment? Find(string code);
    void Save(PayslipAdjustment item);
    bool Remove(string code);
    List<PayslipAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipAdjustmentStore(Dictionary<string, PayslipAdjustment> items, List<string> order) : IPayslipAdjustmentStore
{
    public Dictionary<string, PayslipAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipAdjustment item)
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

    public List<PayslipAdjustment> List()
    {
        var outValue = new List<PayslipAdjustment>();
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
public sealed class CachedPayslipAdjustmentStore(MemoryPayslipAdjustmentStore inner, PayslipAdjustmentCache cache, int hits, int misses) : IPayslipAdjustmentStore
{
    public MemoryPayslipAdjustmentStore Inner { get; set; } = inner;
    public PayslipAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipAdjustment? Find(string code)
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

    public void Save(PayslipAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip adjustments. Empty fields match all.</summary>
public sealed record PayslipAdjustmentQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipAdjustment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipAdjustment> Apply(List<PayslipAdjustment> items)
    {
        var matched = Collections.Filter(items, (PayslipAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip adjustment store.</summary>
public static class PayslipAdjustmentStore
{
    public static IPayslipAdjustmentStore NewMemoryPayslipAdjustmentStore()
    {
        return new MemoryPayslipAdjustmentStore(new Dictionary<string, PayslipAdjustment>(), new List<string>());
    }

    public static IPayslipAdjustmentStore NewCachedPayslipAdjustmentStore(MemoryPayslipAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipAdjustmentStore(inner, PayslipAdjustmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayslipAdjustment> FindPayslipAdjustments(IPayslipAdjustmentStore store, PayslipAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
