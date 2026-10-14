namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip preferences by code.</summary>
public interface IPayslipPreferenceStore
{
    PayslipPreference? Find(string code);
    void Save(PayslipPreference item);
    bool Remove(string code);
    List<PayslipPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipPreferenceStore(Dictionary<string, PayslipPreference> items, List<string> order) : IPayslipPreferenceStore
{
    public Dictionary<string, PayslipPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipPreference item)
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

    public List<PayslipPreference> List()
    {
        var outValue = new List<PayslipPreference>();
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
public sealed class CachedPayslipPreferenceStore(MemoryPayslipPreferenceStore inner, PayslipPreferenceCache cache, int hits, int misses) : IPayslipPreferenceStore
{
    public MemoryPayslipPreferenceStore Inner { get; set; } = inner;
    public PayslipPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipPreference? Find(string code)
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

    public void Save(PayslipPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip preferences. Empty fields match all.</summary>
public sealed record PayslipPreferenceQuery(string Name, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipPreference item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipPreference> Apply(List<PayslipPreference> items)
    {
        var matched = Collections.Filter(items, (PayslipPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip preference store.</summary>
public static class PayslipPreferenceStore
{
    public static IPayslipPreferenceStore NewMemoryPayslipPreferenceStore()
    {
        return new MemoryPayslipPreferenceStore(new Dictionary<string, PayslipPreference>(), new List<string>());
    }

    public static IPayslipPreferenceStore NewCachedPayslipPreferenceStore(MemoryPayslipPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipPreferenceStore(inner, PayslipPreferenceCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayslipPreference> FindPayslipPreferences(IPayslipPreferenceStore store, PayslipPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
