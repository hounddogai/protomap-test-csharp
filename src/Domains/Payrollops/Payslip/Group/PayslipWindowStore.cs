namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip windows by code.</summary>
public interface IPayslipWindowStore
{
    PayslipWindow? Find(string code);
    void Save(PayslipWindow item);
    bool Remove(string code);
    List<PayslipWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipWindowStore(Dictionary<string, PayslipWindow> items, List<string> order) : IPayslipWindowStore
{
    public Dictionary<string, PayslipWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipWindow item)
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

    public List<PayslipWindow> List()
    {
        var outValue = new List<PayslipWindow>();
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
public sealed class CachedPayslipWindowStore(MemoryPayslipWindowStore inner, PayslipWindowCache cache, int hits, int misses) : IPayslipWindowStore
{
    public MemoryPayslipWindowStore Inner { get; set; } = inner;
    public PayslipWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipWindow? Find(string code)
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

    public void Save(PayslipWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip windows. Empty fields match all.</summary>
public sealed record PayslipWindowQuery(string Segment, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipWindow item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipWindow> Apply(List<PayslipWindow> items)
    {
        var matched = Collections.Filter(items, (PayslipWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip window store.</summary>
public static class PayslipWindowStore
{
    public static IPayslipWindowStore NewMemoryPayslipWindowStore()
    {
        return new MemoryPayslipWindowStore(new Dictionary<string, PayslipWindow>(), new List<string>());
    }

    public static IPayslipWindowStore NewCachedPayslipWindowStore(MemoryPayslipWindowStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipWindowStore(inner, PayslipWindowCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayslipWindow> FindPayslipWindows(IPayslipWindowStore store, PayslipWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
