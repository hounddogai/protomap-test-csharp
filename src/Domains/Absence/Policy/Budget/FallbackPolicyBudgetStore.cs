namespace Acme.Workforce.Absence.Policy.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback policy budgets by code.</summary>
public interface IFallbackPolicyBudgetStore
{
    FallbackPolicyBudget? Find(string code);
    void Save(FallbackPolicyBudget item);
    bool Remove(string code);
    List<FallbackPolicyBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPolicyBudgetStore(Dictionary<string, FallbackPolicyBudget> items, List<string> order) : IFallbackPolicyBudgetStore
{
    public Dictionary<string, FallbackPolicyBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPolicyBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPolicyBudget item)
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

    public List<FallbackPolicyBudget> List()
    {
        var outValue = new List<FallbackPolicyBudget>();
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
public sealed class CachedFallbackPolicyBudgetStore(MemoryFallbackPolicyBudgetStore inner, FallbackPolicyBudgetCache cache, int hits, int misses) : IFallbackPolicyBudgetStore
{
    public MemoryFallbackPolicyBudgetStore Inner { get; set; } = inner;
    public FallbackPolicyBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPolicyBudget? Find(string code)
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

    public void Save(FallbackPolicyBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPolicyBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback policy budgets. Empty fields match all.</summary>
public sealed record FallbackPolicyBudgetQuery(string Department, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPolicyBudget item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPolicyBudget> Apply(List<FallbackPolicyBudget> items)
    {
        var matched = Collections.Filter(items, (FallbackPolicyBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback policy budget store.</summary>
public static class FallbackPolicyBudgetStore
{
    public static IFallbackPolicyBudgetStore NewMemoryFallbackPolicyBudgetStore()
    {
        return new MemoryFallbackPolicyBudgetStore(
            new Dictionary<string, FallbackPolicyBudget>(),
            new List<string>());
    }

    public static IFallbackPolicyBudgetStore NewCachedFallbackPolicyBudgetStore(MemoryFallbackPolicyBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPolicyBudgetStore(inner, FallbackPolicyBudgetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<FallbackPolicyBudget> FindFallbackPolicyBudgets(IFallbackPolicyBudgetStore store, FallbackPolicyBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
