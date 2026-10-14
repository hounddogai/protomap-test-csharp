namespace Acme.Workforce.Payrollops.Payslip.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient payslip exceptions by code.</summary>
public interface ILenientPayslipExceptionStore
{
    LenientPayslipException? Find(string code);
    void Save(LenientPayslipException item);
    bool Remove(string code);
    List<LenientPayslipException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPayslipExceptionStore(Dictionary<string, LenientPayslipException> items, List<string> order) : ILenientPayslipExceptionStore
{
    public Dictionary<string, LenientPayslipException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPayslipException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPayslipException item)
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

    public List<LenientPayslipException> List()
    {
        var outValue = new List<LenientPayslipException>();
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
public sealed class CachedLenientPayslipExceptionStore(MemoryLenientPayslipExceptionStore inner, LenientPayslipExceptionCache cache, int hits, int misses) : ILenientPayslipExceptionStore
{
    public MemoryLenientPayslipExceptionStore Inner { get; set; } = inner;
    public LenientPayslipExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPayslipException? Find(string code)
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

    public void Save(LenientPayslipException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPayslipException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient payslip exceptions. Empty fields match all.</summary>
public sealed record LenientPayslipExceptionQuery(string Segment, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPayslipException item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientPayslipException> Apply(List<LenientPayslipException> items)
    {
        var matched = Collections.Filter(items, (LenientPayslipException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient payslip exception store.</summary>
public static class LenientPayslipExceptionStore
{
    public static ILenientPayslipExceptionStore NewMemoryLenientPayslipExceptionStore()
    {
        return new MemoryLenientPayslipExceptionStore(
            new Dictionary<string, LenientPayslipException>(),
            new List<string>());
    }

    public static ILenientPayslipExceptionStore NewCachedLenientPayslipExceptionStore(MemoryLenientPayslipExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPayslipExceptionStore(
            inner,
            LenientPayslipExceptionCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientPayslipException> FindLenientPayslipExceptions(ILenientPayslipExceptionStore store, LenientPayslipExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
