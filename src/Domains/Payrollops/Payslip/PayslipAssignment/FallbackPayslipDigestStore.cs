namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback payslip digests by code.</summary>
public interface IFallbackPayslipDigestStore
{
    FallbackPayslipDigest? Find(string code);
    void Save(FallbackPayslipDigest item);
    bool Remove(string code);
    List<FallbackPayslipDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPayslipDigestStore(Dictionary<string, FallbackPayslipDigest> items, List<string> order) : IFallbackPayslipDigestStore
{
    public Dictionary<string, FallbackPayslipDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPayslipDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPayslipDigest item)
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

    public List<FallbackPayslipDigest> List()
    {
        var outValue = new List<FallbackPayslipDigest>();
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
public sealed class CachedFallbackPayslipDigestStore(MemoryFallbackPayslipDigestStore inner, FallbackPayslipDigestCache cache, int hits, int misses) : IFallbackPayslipDigestStore
{
    public MemoryFallbackPayslipDigestStore Inner { get; set; } = inner;
    public FallbackPayslipDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPayslipDigest? Find(string code)
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

    public void Save(FallbackPayslipDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPayslipDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback payslip digests. Empty fields match all.</summary>
public sealed record FallbackPayslipDigestQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPayslipDigest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPayslipDigest> Apply(List<FallbackPayslipDigest> items)
    {
        var matched = Collections.Filter(items, (FallbackPayslipDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback payslip digest store.</summary>
public static class FallbackPayslipDigestStore
{
    public static IFallbackPayslipDigestStore NewMemoryFallbackPayslipDigestStore()
    {
        return new MemoryFallbackPayslipDigestStore(
            new Dictionary<string, FallbackPayslipDigest>(),
            new List<string>());
    }

    public static IFallbackPayslipDigestStore NewCachedFallbackPayslipDigestStore(MemoryFallbackPayslipDigestStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPayslipDigestStore(inner, FallbackPayslipDigestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<FallbackPayslipDigest> FindFallbackPayslipDigests(IFallbackPayslipDigestStore store, FallbackPayslipDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
