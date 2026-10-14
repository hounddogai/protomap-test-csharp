namespace Acme.Workforce.Payrollops.Payslip.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip assignments by code.</summary>
public interface IPayslipAssignmentStore
{
    PayslipAssignment? Find(string code);
    void Save(PayslipAssignment item);
    bool Remove(string code);
    List<PayslipAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipAssignmentStore(Dictionary<string, PayslipAssignment> items, List<string> order) : IPayslipAssignmentStore
{
    public Dictionary<string, PayslipAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipAssignment item)
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

    public List<PayslipAssignment> List()
    {
        var outValue = new List<PayslipAssignment>();
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
public sealed class CachedPayslipAssignmentStore(MemoryPayslipAssignmentStore inner, PayslipAssignmentCache cache, int hits, int misses) : IPayslipAssignmentStore
{
    public MemoryPayslipAssignmentStore Inner { get; set; } = inner;
    public PayslipAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipAssignment? Find(string code)
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

    public void Save(PayslipAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip assignments. Empty fields match all.</summary>
public sealed record PayslipAssignmentQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipAssignment> Apply(List<PayslipAssignment> items)
    {
        var matched = Collections.Filter(items, (PayslipAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip assignment store.</summary>
public static class PayslipAssignmentStore
{
    public static IPayslipAssignmentStore NewMemoryPayslipAssignmentStore()
    {
        return new MemoryPayslipAssignmentStore(new Dictionary<string, PayslipAssignment>(), new List<string>());
    }

    public static IPayslipAssignmentStore NewCachedPayslipAssignmentStore(MemoryPayslipAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipAssignmentStore(inner, PayslipAssignmentCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayslipAssignment> FindPayslipAssignments(IPayslipAssignmentStore store, PayslipAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
