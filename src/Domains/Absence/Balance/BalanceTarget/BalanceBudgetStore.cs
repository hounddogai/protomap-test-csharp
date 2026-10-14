namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance budgets by code.</summary>
public interface IBalanceBudgetStore
{
    BalanceBudget? Find(string code);
    void Save(BalanceBudget item);
    bool Remove(string code);
    List<BalanceBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceBudgetStore(Dictionary<string, BalanceBudget> items, List<string> order) : IBalanceBudgetStore
{
    public Dictionary<string, BalanceBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceBudget item)
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

    public List<BalanceBudget> List()
    {
        var outValue = new List<BalanceBudget>();
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
public sealed class CachedBalanceBudgetStore(MemoryBalanceBudgetStore inner, BalanceBudgetCache cache, int hits, int misses) : IBalanceBudgetStore
{
    public MemoryBalanceBudgetStore Inner { get; set; } = inner;
    public BalanceBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceBudget? Find(string code)
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

    public void Save(BalanceBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance budgets. Empty fields match all.</summary>
public sealed record BalanceBudgetQuery(string Label, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceBudget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceBudget> Apply(List<BalanceBudget> items)
    {
        var matched = Collections.Filter(items, (BalanceBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance budget store.</summary>
public static class BalanceBudgetStore
{
    public static IBalanceBudgetStore NewMemoryBalanceBudgetStore()
    {
        return new MemoryBalanceBudgetStore(new Dictionary<string, BalanceBudget>(), new List<string>());
    }

    public static IBalanceBudgetStore NewCachedBalanceBudgetStore(MemoryBalanceBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceBudgetStore(inner, BalanceBudgetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BalanceBudget> FindBalanceBudgets(IBalanceBudgetStore store, BalanceBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
