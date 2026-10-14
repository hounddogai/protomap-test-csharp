namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay budgets by code.</summary>
public interface IRetroPayBudgetStore
{
    RetroPayBudget? Find(string code);
    void Save(RetroPayBudget item);
    bool Remove(string code);
    List<RetroPayBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayBudgetStore(Dictionary<string, RetroPayBudget> items, List<string> order) : IRetroPayBudgetStore
{
    public Dictionary<string, RetroPayBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayBudget item)
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

    public List<RetroPayBudget> List()
    {
        var outValue = new List<RetroPayBudget>();
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
public sealed class CachedRetroPayBudgetStore(MemoryRetroPayBudgetStore inner, RetroPayBudgetCache cache, int hits, int misses) : IRetroPayBudgetStore
{
    public MemoryRetroPayBudgetStore Inner { get; set; } = inner;
    public RetroPayBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayBudget? Find(string code)
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

    public void Save(RetroPayBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay budgets. Empty fields match all.</summary>
public sealed record RetroPayBudgetQuery(string Team, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayBudget item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayBudget> Apply(List<RetroPayBudget> items)
    {
        var matched = Collections.Filter(items, (RetroPayBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay budget store.</summary>
public static class RetroPayBudgetStore
{
    public static IRetroPayBudgetStore NewMemoryRetroPayBudgetStore()
    {
        return new MemoryRetroPayBudgetStore(new Dictionary<string, RetroPayBudget>(), new List<string>());
    }

    public static IRetroPayBudgetStore NewCachedRetroPayBudgetStore(MemoryRetroPayBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayBudgetStore(inner, RetroPayBudgetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RetroPayBudget> FindRetroPayBudgets(IRetroPayBudgetStore store, RetroPayBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
