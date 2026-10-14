namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default payslip budgets by code.</summary>
public interface IDefaultPayslipBudgetStore
{
    DefaultPayslipBudget? Find(string code);
    void Save(DefaultPayslipBudget item);
    bool Remove(string code);
    List<DefaultPayslipBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPayslipBudgetStore(Dictionary<string, DefaultPayslipBudget> items, List<string> order) : IDefaultPayslipBudgetStore
{
    public Dictionary<string, DefaultPayslipBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPayslipBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPayslipBudget item)
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

    public List<DefaultPayslipBudget> List()
    {
        var outValue = new List<DefaultPayslipBudget>();
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
public sealed class CachedDefaultPayslipBudgetStore(MemoryDefaultPayslipBudgetStore inner, DefaultPayslipBudgetCache cache, int hits, int misses) : IDefaultPayslipBudgetStore
{
    public MemoryDefaultPayslipBudgetStore Inner { get; set; } = inner;
    public DefaultPayslipBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPayslipBudget? Find(string code)
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

    public void Save(DefaultPayslipBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPayslipBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default payslip budgets. Empty fields match all.</summary>
public sealed record DefaultPayslipBudgetQuery(string Title, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPayslipBudget item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultPayslipBudget> Apply(List<DefaultPayslipBudget> items)
    {
        var matched = Collections.Filter(items, (DefaultPayslipBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default payslip budget store.</summary>
public static class DefaultPayslipBudgetStore
{
    public static IDefaultPayslipBudgetStore NewMemoryDefaultPayslipBudgetStore()
    {
        return new MemoryDefaultPayslipBudgetStore(
            new Dictionary<string, DefaultPayslipBudget>(),
            new List<string>());
    }

    public static IDefaultPayslipBudgetStore NewCachedDefaultPayslipBudgetStore(MemoryDefaultPayslipBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPayslipBudgetStore(inner, DefaultPayslipBudgetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DefaultPayslipBudget> FindDefaultPayslipBudgets(IDefaultPayslipBudgetStore store, DefaultPayslipBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
