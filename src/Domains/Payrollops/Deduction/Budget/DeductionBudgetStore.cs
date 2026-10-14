namespace Acme.Workforce.Payrollops.Deduction.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction budgets by code.</summary>
public interface IDeductionBudgetStore
{
    DeductionBudget? Find(string code);
    void Save(DeductionBudget item);
    bool Remove(string code);
    List<DeductionBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionBudgetStore(Dictionary<string, DeductionBudget> items, List<string> order) : IDeductionBudgetStore
{
    public Dictionary<string, DeductionBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionBudget item)
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

    public List<DeductionBudget> List()
    {
        var outValue = new List<DeductionBudget>();
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
public sealed class CachedDeductionBudgetStore(MemoryDeductionBudgetStore inner, DeductionBudgetCache cache, int hits, int misses) : IDeductionBudgetStore
{
    public MemoryDeductionBudgetStore Inner { get; set; } = inner;
    public DeductionBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionBudget? Find(string code)
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

    public void Save(DeductionBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction budgets. Empty fields match all.</summary>
public sealed record DeductionBudgetQuery(string Department, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionBudget item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionBudget> Apply(List<DeductionBudget> items)
    {
        var matched = Collections.Filter(items, (DeductionBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction budget store.</summary>
public static class DeductionBudgetStore
{
    public static IDeductionBudgetStore NewMemoryDeductionBudgetStore()
    {
        return new MemoryDeductionBudgetStore(new Dictionary<string, DeductionBudget>(), new List<string>());
    }

    public static IDeductionBudgetStore NewCachedDeductionBudgetStore(MemoryDeductionBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionBudgetStore(inner, DeductionBudgetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeductionBudget> FindDeductionBudgets(IDeductionBudgetStore store, DeductionBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
