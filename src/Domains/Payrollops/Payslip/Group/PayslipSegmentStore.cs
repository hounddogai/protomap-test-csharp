namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip segments by code.</summary>
public interface IPayslipSegmentStore
{
    PayslipSegment? Find(string code);
    void Save(PayslipSegment item);
    bool Remove(string code);
    List<PayslipSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipSegmentStore(Dictionary<string, PayslipSegment> items, List<string> order) : IPayslipSegmentStore
{
    public Dictionary<string, PayslipSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipSegment item)
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

    public List<PayslipSegment> List()
    {
        var outValue = new List<PayslipSegment>();
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
public sealed class CachedPayslipSegmentStore(MemoryPayslipSegmentStore inner, PayslipSegmentCache cache, int hits, int misses) : IPayslipSegmentStore
{
    public MemoryPayslipSegmentStore Inner { get; set; } = inner;
    public PayslipSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipSegment? Find(string code)
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

    public void Save(PayslipSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip segments. Empty fields match all.</summary>
public sealed record PayslipSegmentQuery(string Region, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipSegment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<PayslipSegment> Apply(List<PayslipSegment> items)
    {
        var matched = Collections.Filter(items, (PayslipSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip segment store.</summary>
public static class PayslipSegmentStore
{
    public static IPayslipSegmentStore NewMemoryPayslipSegmentStore()
    {
        return new MemoryPayslipSegmentStore(new Dictionary<string, PayslipSegment>(), new List<string>());
    }

    public static IPayslipSegmentStore NewCachedPayslipSegmentStore(MemoryPayslipSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipSegmentStore(inner, PayslipSegmentCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayslipSegment> FindPayslipSegments(IPayslipSegmentStore store, PayslipSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
