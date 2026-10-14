namespace Acme.Workforce.Contractors.Timesheet.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered timesheet budgets by code.</summary>
public interface ITieredTimesheetBudgetStore
{
    TieredTimesheetBudget? Find(string code);
    void Save(TieredTimesheetBudget item);
    bool Remove(string code);
    List<TieredTimesheetBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredTimesheetBudgetStore(Dictionary<string, TieredTimesheetBudget> items, List<string> order) : ITieredTimesheetBudgetStore
{
    public Dictionary<string, TieredTimesheetBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredTimesheetBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredTimesheetBudget item)
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

    public List<TieredTimesheetBudget> List()
    {
        var outValue = new List<TieredTimesheetBudget>();
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
public sealed class CachedTieredTimesheetBudgetStore(MemoryTieredTimesheetBudgetStore inner, TieredTimesheetBudgetCache cache, int hits, int misses) : ITieredTimesheetBudgetStore
{
    public MemoryTieredTimesheetBudgetStore Inner { get; set; } = inner;
    public TieredTimesheetBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredTimesheetBudget? Find(string code)
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

    public void Save(TieredTimesheetBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredTimesheetBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered timesheet budgets. Empty fields match all.</summary>
public sealed record TieredTimesheetBudgetQuery(string Department, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(TieredTimesheetBudget item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredTimesheetBudget> Apply(List<TieredTimesheetBudget> items)
    {
        var matched = Collections.Filter(items, (TieredTimesheetBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered timesheet budget store.</summary>
public static class TieredTimesheetBudgetStore
{
    public static ITieredTimesheetBudgetStore NewMemoryTieredTimesheetBudgetStore()
    {
        return new MemoryTieredTimesheetBudgetStore(
            new Dictionary<string, TieredTimesheetBudget>(),
            new List<string>());
    }

    public static ITieredTimesheetBudgetStore NewCachedTieredTimesheetBudgetStore(MemoryTieredTimesheetBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedTieredTimesheetBudgetStore(
            inner,
            TieredTimesheetBudgetCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TieredTimesheetBudget> FindTieredTimesheetBudgets(ITieredTimesheetBudgetStore store, TieredTimesheetBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
