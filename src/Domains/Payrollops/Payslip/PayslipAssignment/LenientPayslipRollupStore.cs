namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient payslip rollups by code.</summary>
public interface ILenientPayslipRollupStore
{
    LenientPayslipRollup? Find(string code);
    void Save(LenientPayslipRollup item);
    bool Remove(string code);
    List<LenientPayslipRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPayslipRollupStore(Dictionary<string, LenientPayslipRollup> items, List<string> order) : ILenientPayslipRollupStore
{
    public Dictionary<string, LenientPayslipRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPayslipRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPayslipRollup item)
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

    public List<LenientPayslipRollup> List()
    {
        var outValue = new List<LenientPayslipRollup>();
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
public sealed class CachedLenientPayslipRollupStore(MemoryLenientPayslipRollupStore inner, LenientPayslipRollupCache cache, int hits, int misses) : ILenientPayslipRollupStore
{
    public MemoryLenientPayslipRollupStore Inner { get; set; } = inner;
    public LenientPayslipRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPayslipRollup? Find(string code)
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

    public void Save(LenientPayslipRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPayslipRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient payslip rollups. Empty fields match all.</summary>
public sealed record LenientPayslipRollupQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPayslipRollup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientPayslipRollup> Apply(List<LenientPayslipRollup> items)
    {
        var matched = Collections.Filter(items, (LenientPayslipRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient payslip rollup store.</summary>
public static class LenientPayslipRollupStore
{
    public static ILenientPayslipRollupStore NewMemoryLenientPayslipRollupStore()
    {
        return new MemoryLenientPayslipRollupStore(
            new Dictionary<string, LenientPayslipRollup>(),
            new List<string>());
    }

    public static ILenientPayslipRollupStore NewCachedLenientPayslipRollupStore(MemoryLenientPayslipRollupStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPayslipRollupStore(inner, LenientPayslipRollupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LenientPayslipRollup> FindLenientPayslipRollups(ILenientPayslipRollupStore store, LenientPayslipRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
