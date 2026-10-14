namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan budgets by code.</summary>
public interface ISuccessionPlanBudgetStore
{
    SuccessionPlanBudget? Find(string code);
    void Save(SuccessionPlanBudget item);
    bool Remove(string code);
    List<SuccessionPlanBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanBudgetStore(Dictionary<string, SuccessionPlanBudget> items, List<string> order) : ISuccessionPlanBudgetStore
{
    public Dictionary<string, SuccessionPlanBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanBudget item)
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

    public List<SuccessionPlanBudget> List()
    {
        var outValue = new List<SuccessionPlanBudget>();
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
public sealed class CachedSuccessionPlanBudgetStore(MemorySuccessionPlanBudgetStore inner, SuccessionPlanBudgetCache cache, int hits, int misses) : ISuccessionPlanBudgetStore
{
    public MemorySuccessionPlanBudgetStore Inner { get; set; } = inner;
    public SuccessionPlanBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanBudget? Find(string code)
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

    public void Save(SuccessionPlanBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan budgets. Empty fields match all.</summary>
public sealed record SuccessionPlanBudgetQuery(string Name, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanBudget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<SuccessionPlanBudget> Apply(List<SuccessionPlanBudget> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan budget store.</summary>
public static class SuccessionPlanBudgetStore
{
    public static ISuccessionPlanBudgetStore NewMemorySuccessionPlanBudgetStore()
    {
        return new MemorySuccessionPlanBudgetStore(
            new Dictionary<string, SuccessionPlanBudget>(),
            new List<string>());
    }

    public static ISuccessionPlanBudgetStore NewCachedSuccessionPlanBudgetStore(MemorySuccessionPlanBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanBudgetStore(inner, SuccessionPlanBudgetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SuccessionPlanBudget> FindSuccessionPlanBudgets(ISuccessionPlanBudgetStore store, SuccessionPlanBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
