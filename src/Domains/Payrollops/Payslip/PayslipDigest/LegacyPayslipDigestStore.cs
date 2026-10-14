namespace Acme.Workforce.Payrollops.Payslip.PayslipDigest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy payslip digests by code.</summary>
public interface ILegacyPayslipDigestStore
{
    LegacyPayslipDigest? Find(string code);
    void Save(LegacyPayslipDigest item);
    bool Remove(string code);
    List<LegacyPayslipDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPayslipDigestStore(Dictionary<string, LegacyPayslipDigest> items, List<string> order) : ILegacyPayslipDigestStore
{
    public Dictionary<string, LegacyPayslipDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPayslipDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPayslipDigest item)
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

    public List<LegacyPayslipDigest> List()
    {
        var outValue = new List<LegacyPayslipDigest>();
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
public sealed class CachedLegacyPayslipDigestStore(MemoryLegacyPayslipDigestStore inner, LegacyPayslipDigestCache cache, int hits, int misses) : ILegacyPayslipDigestStore
{
    public MemoryLegacyPayslipDigestStore Inner { get; set; } = inner;
    public LegacyPayslipDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPayslipDigest? Find(string code)
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

    public void Save(LegacyPayslipDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPayslipDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy payslip digests. Empty fields match all.</summary>
public sealed record LegacyPayslipDigestQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPayslipDigest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<LegacyPayslipDigest> Apply(List<LegacyPayslipDigest> items)
    {
        var matched = Collections.Filter(items, (LegacyPayslipDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy payslip digest store.</summary>
public static class LegacyPayslipDigestStore
{
    public static ILegacyPayslipDigestStore NewMemoryLegacyPayslipDigestStore()
    {
        return new MemoryLegacyPayslipDigestStore(new Dictionary<string, LegacyPayslipDigest>(), new List<string>());
    }

    public static ILegacyPayslipDigestStore NewCachedLegacyPayslipDigestStore(MemoryLegacyPayslipDigestStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPayslipDigestStore(inner, LegacyPayslipDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LegacyPayslipDigest> FindLegacyPayslipDigests(ILegacyPayslipDigestStore store, LegacyPayslipDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
