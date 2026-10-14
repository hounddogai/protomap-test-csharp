namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pool budgets by code.</summary>
public interface IPoolBudgetStore
{
    PoolBudget? Find(string code);
    void Save(PoolBudget item);
    bool Remove(string code);
    List<PoolBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPoolBudgetStore(Dictionary<string, PoolBudget> items, List<string> order) : IPoolBudgetStore
{
    public Dictionary<string, PoolBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PoolBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PoolBudget item)
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

    public List<PoolBudget> List()
    {
        var outValue = new List<PoolBudget>();
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
public sealed class CachedPoolBudgetStore(MemoryPoolBudgetStore inner, PoolBudgetCache cache, int hits, int misses) : IPoolBudgetStore
{
    public MemoryPoolBudgetStore Inner { get; set; } = inner;
    public PoolBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PoolBudget? Find(string code)
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

    public void Save(PoolBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PoolBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pool budgets. Empty fields match all.</summary>
public sealed record PoolBudgetQuery(string Locale, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(PoolBudget item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PoolBudget> Apply(List<PoolBudget> items)
    {
        var matched = Collections.Filter(items, (PoolBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pool budget store.</summary>
public static class PoolBudgetStore
{
    public static IPoolBudgetStore NewMemoryPoolBudgetStore()
    {
        return new MemoryPoolBudgetStore(new Dictionary<string, PoolBudget>(), new List<string>());
    }

    public static IPoolBudgetStore NewCachedPoolBudgetStore(MemoryPoolBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedPoolBudgetStore(inner, PoolBudgetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PoolBudget> FindPoolBudgets(IPoolBudgetStore store, PoolBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
