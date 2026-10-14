namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default payslip thresholds by code.</summary>
public interface IDefaultPayslipThresholdStore
{
    DefaultPayslipThreshold? Find(string code);
    void Save(DefaultPayslipThreshold item);
    bool Remove(string code);
    List<DefaultPayslipThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPayslipThresholdStore(Dictionary<string, DefaultPayslipThreshold> items, List<string> order) : IDefaultPayslipThresholdStore
{
    public Dictionary<string, DefaultPayslipThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPayslipThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPayslipThreshold item)
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

    public List<DefaultPayslipThreshold> List()
    {
        var outValue = new List<DefaultPayslipThreshold>();
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
public sealed class CachedDefaultPayslipThresholdStore(MemoryDefaultPayslipThresholdStore inner, DefaultPayslipThresholdCache cache, int hits, int misses) : IDefaultPayslipThresholdStore
{
    public MemoryDefaultPayslipThresholdStore Inner { get; set; } = inner;
    public DefaultPayslipThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPayslipThreshold? Find(string code)
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

    public void Save(DefaultPayslipThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPayslipThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default payslip thresholds. Empty fields match all.</summary>
public sealed record DefaultPayslipThresholdQuery(string Country, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPayslipThreshold item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<DefaultPayslipThreshold> Apply(List<DefaultPayslipThreshold> items)
    {
        var matched = Collections.Filter(items, (DefaultPayslipThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default payslip threshold store.</summary>
public static class DefaultPayslipThresholdStore
{
    public static IDefaultPayslipThresholdStore NewMemoryDefaultPayslipThresholdStore()
    {
        return new MemoryDefaultPayslipThresholdStore(
            new Dictionary<string, DefaultPayslipThreshold>(),
            new List<string>());
    }

    public static IDefaultPayslipThresholdStore NewCachedDefaultPayslipThresholdStore(MemoryDefaultPayslipThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPayslipThresholdStore(
            inner,
            DefaultPayslipThresholdCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultPayslipThreshold> FindDefaultPayslipThresholds(IDefaultPayslipThresholdStore store, DefaultPayslipThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
