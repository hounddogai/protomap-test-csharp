namespace Acme.Workforce.Payrollops.Payslip.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip exceptions by code.</summary>
public interface IPayslipExceptionStore
{
    PayslipException? Find(string code);
    void Save(PayslipException item);
    bool Remove(string code);
    List<PayslipException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipExceptionStore(Dictionary<string, PayslipException> items, List<string> order) : IPayslipExceptionStore
{
    public Dictionary<string, PayslipException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipException item)
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

    public List<PayslipException> List()
    {
        var outValue = new List<PayslipException>();
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
public sealed class CachedPayslipExceptionStore(MemoryPayslipExceptionStore inner, PayslipExceptionCache cache, int hits, int misses) : IPayslipExceptionStore
{
    public MemoryPayslipExceptionStore Inner { get; set; } = inner;
    public PayslipExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipException? Find(string code)
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

    public void Save(PayslipException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip exceptions. Empty fields match all.</summary>
public sealed record PayslipExceptionQuery(string Channel, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipException item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<PayslipException> Apply(List<PayslipException> items)
    {
        var matched = Collections.Filter(items, (PayslipException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip exception store.</summary>
public static class PayslipExceptionStore
{
    public static IPayslipExceptionStore NewMemoryPayslipExceptionStore()
    {
        return new MemoryPayslipExceptionStore(new Dictionary<string, PayslipException>(), new List<string>());
    }

    public static IPayslipExceptionStore NewCachedPayslipExceptionStore(MemoryPayslipExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipExceptionStore(inner, PayslipExceptionCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayslipException> FindPayslipExceptions(IPayslipExceptionStore store, PayslipExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
