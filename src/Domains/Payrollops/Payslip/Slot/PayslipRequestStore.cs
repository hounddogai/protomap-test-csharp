namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip requests by code.</summary>
public interface IPayslipRequestStore
{
    PayslipRequest? Find(string code);
    void Save(PayslipRequest item);
    bool Remove(string code);
    List<PayslipRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipRequestStore(Dictionary<string, PayslipRequest> items, List<string> order) : IPayslipRequestStore
{
    public Dictionary<string, PayslipRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipRequest item)
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

    public List<PayslipRequest> List()
    {
        var outValue = new List<PayslipRequest>();
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
public sealed class CachedPayslipRequestStore(MemoryPayslipRequestStore inner, PayslipRequestCache cache, int hits, int misses) : IPayslipRequestStore
{
    public MemoryPayslipRequestStore Inner { get; set; } = inner;
    public PayslipRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipRequest? Find(string code)
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

    public void Save(PayslipRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip requests. Empty fields match all.</summary>
public sealed record PayslipRequestQuery(string Category, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipRequest item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipRequest> Apply(List<PayslipRequest> items)
    {
        var matched = Collections.Filter(items, (PayslipRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip request store.</summary>
public static class PayslipRequestStore
{
    public static IPayslipRequestStore NewMemoryPayslipRequestStore()
    {
        return new MemoryPayslipRequestStore(new Dictionary<string, PayslipRequest>(), new List<string>());
    }

    public static IPayslipRequestStore NewCachedPayslipRequestStore(MemoryPayslipRequestStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipRequestStore(inner, PayslipRequestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayslipRequest> FindPayslipRequests(IPayslipRequestStore store, PayslipRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
