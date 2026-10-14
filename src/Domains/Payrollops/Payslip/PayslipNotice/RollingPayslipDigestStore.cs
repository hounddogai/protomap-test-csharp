namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling payslip digests by code.</summary>
public interface IRollingPayslipDigestStore
{
    RollingPayslipDigest? Find(string code);
    void Save(RollingPayslipDigest item);
    bool Remove(string code);
    List<RollingPayslipDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingPayslipDigestStore(Dictionary<string, RollingPayslipDigest> items, List<string> order) : IRollingPayslipDigestStore
{
    public Dictionary<string, RollingPayslipDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingPayslipDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingPayslipDigest item)
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

    public List<RollingPayslipDigest> List()
    {
        var outValue = new List<RollingPayslipDigest>();
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
public sealed class CachedRollingPayslipDigestStore(MemoryRollingPayslipDigestStore inner, RollingPayslipDigestCache cache, int hits, int misses) : IRollingPayslipDigestStore
{
    public MemoryRollingPayslipDigestStore Inner { get; set; } = inner;
    public RollingPayslipDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingPayslipDigest? Find(string code)
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

    public void Save(RollingPayslipDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingPayslipDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling payslip digests. Empty fields match all.</summary>
public sealed record RollingPayslipDigestQuery(string Channel, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(RollingPayslipDigest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<RollingPayslipDigest> Apply(List<RollingPayslipDigest> items)
    {
        var matched = Collections.Filter(items, (RollingPayslipDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling payslip digest store.</summary>
public static class RollingPayslipDigestStore
{
    public static IRollingPayslipDigestStore NewMemoryRollingPayslipDigestStore()
    {
        return new MemoryRollingPayslipDigestStore(
            new Dictionary<string, RollingPayslipDigest>(),
            new List<string>());
    }

    public static IRollingPayslipDigestStore NewCachedRollingPayslipDigestStore(MemoryRollingPayslipDigestStore inner, int ttl, IClock clock)
    {
        return new CachedRollingPayslipDigestStore(inner, RollingPayslipDigestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RollingPayslipDigest> FindRollingPayslipDigests(IRollingPayslipDigestStore store, RollingPayslipDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
