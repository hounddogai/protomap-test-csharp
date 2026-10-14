namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip slots by code.</summary>
public interface IPayslipSlotStore
{
    PayslipSlot? Find(string code);
    void Save(PayslipSlot item);
    bool Remove(string code);
    List<PayslipSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipSlotStore(Dictionary<string, PayslipSlot> items, List<string> order) : IPayslipSlotStore
{
    public Dictionary<string, PayslipSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipSlot item)
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

    public List<PayslipSlot> List()
    {
        var outValue = new List<PayslipSlot>();
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
public sealed class CachedPayslipSlotStore(MemoryPayslipSlotStore inner, PayslipSlotCache cache, int hits, int misses) : IPayslipSlotStore
{
    public MemoryPayslipSlotStore Inner { get; set; } = inner;
    public PayslipSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipSlot? Find(string code)
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

    public void Save(PayslipSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip slots. Empty fields match all.</summary>
public sealed record PayslipSlotQuery(string Department, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipSlot item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipSlot> Apply(List<PayslipSlot> items)
    {
        var matched = Collections.Filter(items, (PayslipSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip slot store.</summary>
public static class PayslipSlotStore
{
    public static IPayslipSlotStore NewMemoryPayslipSlotStore()
    {
        return new MemoryPayslipSlotStore(new Dictionary<string, PayslipSlot>(), new List<string>());
    }

    public static IPayslipSlotStore NewCachedPayslipSlotStore(MemoryPayslipSlotStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipSlotStore(inner, PayslipSlotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayslipSlot> FindPayslipSlots(IPayslipSlotStore store, PayslipSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
