namespace Acme.Workforce.Payrollops.Payslip.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted payslip budgets by code.</summary>
public interface IWeightedPayslipBudgetStore
{
    WeightedPayslipBudget? Find(string code);
    void Save(WeightedPayslipBudget item);
    bool Remove(string code);
    List<WeightedPayslipBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedPayslipBudgetStore(Dictionary<string, WeightedPayslipBudget> items, List<string> order) : IWeightedPayslipBudgetStore
{
    public Dictionary<string, WeightedPayslipBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedPayslipBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedPayslipBudget item)
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

    public List<WeightedPayslipBudget> List()
    {
        var outValue = new List<WeightedPayslipBudget>();
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
public sealed class CachedWeightedPayslipBudgetStore(MemoryWeightedPayslipBudgetStore inner, WeightedPayslipBudgetCache cache, int hits, int misses) : IWeightedPayslipBudgetStore
{
    public MemoryWeightedPayslipBudgetStore Inner { get; set; } = inner;
    public WeightedPayslipBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedPayslipBudget? Find(string code)
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

    public void Save(WeightedPayslipBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedPayslipBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted payslip budgets. Empty fields match all.</summary>
public sealed record WeightedPayslipBudgetQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedPayslipBudget item)
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

    public List<WeightedPayslipBudget> Apply(List<WeightedPayslipBudget> items)
    {
        var matched = Collections.Filter(items, (WeightedPayslipBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted payslip budget store.</summary>
public static class WeightedPayslipBudgetStore
{
    public static IWeightedPayslipBudgetStore NewMemoryWeightedPayslipBudgetStore()
    {
        return new MemoryWeightedPayslipBudgetStore(
            new Dictionary<string, WeightedPayslipBudget>(),
            new List<string>());
    }

    public static IWeightedPayslipBudgetStore NewCachedWeightedPayslipBudgetStore(MemoryWeightedPayslipBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedPayslipBudgetStore(inner, WeightedPayslipBudgetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<WeightedPayslipBudget> FindWeightedPayslipBudgets(IWeightedPayslipBudgetStore store, WeightedPayslipBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
