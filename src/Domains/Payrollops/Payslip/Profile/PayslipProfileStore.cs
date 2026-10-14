namespace Acme.Workforce.Payrollops.Payslip.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip profiles by code.</summary>
public interface IPayslipProfileStore
{
    PayslipProfile? Find(string code);
    void Save(PayslipProfile item);
    bool Remove(string code);
    List<PayslipProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipProfileStore(Dictionary<string, PayslipProfile> items, List<string> order) : IPayslipProfileStore
{
    public Dictionary<string, PayslipProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipProfile item)
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

    public List<PayslipProfile> List()
    {
        var outValue = new List<PayslipProfile>();
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
public sealed class CachedPayslipProfileStore(MemoryPayslipProfileStore inner, PayslipProfileCache cache, int hits, int misses) : IPayslipProfileStore
{
    public MemoryPayslipProfileStore Inner { get; set; } = inner;
    public PayslipProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipProfile? Find(string code)
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

    public void Save(PayslipProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip profiles. Empty fields match all.</summary>
public sealed record PayslipProfileQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipProfile item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipProfile> Apply(List<PayslipProfile> items)
    {
        var matched = Collections.Filter(items, (PayslipProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip profile store.</summary>
public static class PayslipProfileStore
{
    public static IPayslipProfileStore NewMemoryPayslipProfileStore()
    {
        return new MemoryPayslipProfileStore(new Dictionary<string, PayslipProfile>(), new List<string>());
    }

    public static IPayslipProfileStore NewCachedPayslipProfileStore(MemoryPayslipProfileStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipProfileStore(inner, PayslipProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayslipProfile> FindPayslipProfiles(IPayslipProfileStore store, PayslipProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
