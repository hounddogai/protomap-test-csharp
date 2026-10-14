namespace Acme.Workforce.Contractors.Rate.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate budgets by code.</summary>
public interface IRateBudgetStore
{
    RateBudget? Find(string code);
    void Save(RateBudget item);
    bool Remove(string code);
    List<RateBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateBudgetStore(Dictionary<string, RateBudget> items, List<string> order) : IRateBudgetStore
{
    public Dictionary<string, RateBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateBudget item)
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

    public List<RateBudget> List()
    {
        var outValue = new List<RateBudget>();
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
public sealed class CachedRateBudgetStore(MemoryRateBudgetStore inner, RateBudgetCache cache, int hits, int misses) : IRateBudgetStore
{
    public MemoryRateBudgetStore Inner { get; set; } = inner;
    public RateBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateBudget? Find(string code)
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

    public void Save(RateBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate budgets. Empty fields match all.</summary>
public sealed record RateBudgetQuery(string Name, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(RateBudget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<RateBudget> Apply(List<RateBudget> items)
    {
        var matched = Collections.Filter(items, (RateBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate budget store.</summary>
public static class RateBudgetStore
{
    public static IRateBudgetStore NewMemoryRateBudgetStore()
    {
        return new MemoryRateBudgetStore(new Dictionary<string, RateBudget>(), new List<string>());
    }

    public static IRateBudgetStore NewCachedRateBudgetStore(MemoryRateBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedRateBudgetStore(inner, RateBudgetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RateBudget> FindRateBudgets(IRateBudgetStore store, RateBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
