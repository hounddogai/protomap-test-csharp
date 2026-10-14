namespace Acme.Workforce.Payrollops.Payslip.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped payslip rules by code.</summary>
public interface ICappedPayslipRuleStore
{
    CappedPayslipRule? Find(string code);
    void Save(CappedPayslipRule item);
    bool Remove(string code);
    List<CappedPayslipRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedPayslipRuleStore(Dictionary<string, CappedPayslipRule> items, List<string> order) : ICappedPayslipRuleStore
{
    public Dictionary<string, CappedPayslipRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedPayslipRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedPayslipRule item)
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

    public List<CappedPayslipRule> List()
    {
        var outValue = new List<CappedPayslipRule>();
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
public sealed class CachedCappedPayslipRuleStore(MemoryCappedPayslipRuleStore inner, CappedPayslipRuleCache cache, int hits, int misses) : ICappedPayslipRuleStore
{
    public MemoryCappedPayslipRuleStore Inner { get; set; } = inner;
    public CappedPayslipRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedPayslipRule? Find(string code)
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

    public void Save(CappedPayslipRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedPayslipRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped payslip rules. Empty fields match all.</summary>
public sealed record CappedPayslipRuleQuery(string Title, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedPayslipRule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedPayslipRule> Apply(List<CappedPayslipRule> items)
    {
        var matched = Collections.Filter(items, (CappedPayslipRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped payslip rule store.</summary>
public static class CappedPayslipRuleStore
{
    public static ICappedPayslipRuleStore NewMemoryCappedPayslipRuleStore()
    {
        return new MemoryCappedPayslipRuleStore(new Dictionary<string, CappedPayslipRule>(), new List<string>());
    }

    public static ICappedPayslipRuleStore NewCachedCappedPayslipRuleStore(MemoryCappedPayslipRuleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedPayslipRuleStore(inner, CappedPayslipRuleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CappedPayslipRule> FindCappedPayslipRules(ICappedPayslipRuleStore store, CappedPayslipRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
