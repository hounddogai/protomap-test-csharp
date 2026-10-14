namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional payslip assignments by code.</summary>
public interface IRegionalPayslipAssignmentStore
{
    RegionalPayslipAssignment? Find(string code);
    void Save(RegionalPayslipAssignment item);
    bool Remove(string code);
    List<RegionalPayslipAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPayslipAssignmentStore(Dictionary<string, RegionalPayslipAssignment> items, List<string> order) : IRegionalPayslipAssignmentStore
{
    public Dictionary<string, RegionalPayslipAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPayslipAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPayslipAssignment item)
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

    public List<RegionalPayslipAssignment> List()
    {
        var outValue = new List<RegionalPayslipAssignment>();
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
public sealed class CachedRegionalPayslipAssignmentStore(MemoryRegionalPayslipAssignmentStore inner, RegionalPayslipAssignmentCache cache, int hits, int misses) : IRegionalPayslipAssignmentStore
{
    public MemoryRegionalPayslipAssignmentStore Inner { get; set; } = inner;
    public RegionalPayslipAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPayslipAssignment? Find(string code)
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

    public void Save(RegionalPayslipAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPayslipAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional payslip assignments. Empty fields match all.</summary>
public sealed record RegionalPayslipAssignmentQuery(string Locale, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPayslipAssignment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalPayslipAssignment> Apply(List<RegionalPayslipAssignment> items)
    {
        var matched = Collections.Filter(items, (RegionalPayslipAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional payslip assignment store.</summary>
public static class RegionalPayslipAssignmentStore
{
    public static IRegionalPayslipAssignmentStore NewMemoryRegionalPayslipAssignmentStore()
    {
        return new MemoryRegionalPayslipAssignmentStore(
            new Dictionary<string, RegionalPayslipAssignment>(),
            new List<string>());
    }

    public static IRegionalPayslipAssignmentStore NewCachedRegionalPayslipAssignmentStore(MemoryRegionalPayslipAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPayslipAssignmentStore(
            inner,
            RegionalPayslipAssignmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalPayslipAssignment> FindRegionalPayslipAssignments(IRegionalPayslipAssignmentStore store, RegionalPayslipAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
