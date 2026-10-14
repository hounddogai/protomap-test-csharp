namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict reorg budgets by code.</summary>
public interface IStrictReorgBudgetStore
{
    StrictReorgBudget? Find(string code);
    void Save(StrictReorgBudget item);
    bool Remove(string code);
    List<StrictReorgBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictReorgBudgetStore(Dictionary<string, StrictReorgBudget> items, List<string> order) : IStrictReorgBudgetStore
{
    public Dictionary<string, StrictReorgBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictReorgBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictReorgBudget item)
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

    public List<StrictReorgBudget> List()
    {
        var outValue = new List<StrictReorgBudget>();
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
public sealed class CachedStrictReorgBudgetStore(MemoryStrictReorgBudgetStore inner, StrictReorgBudgetCache cache, int hits, int misses) : IStrictReorgBudgetStore
{
    public MemoryStrictReorgBudgetStore Inner { get; set; } = inner;
    public StrictReorgBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictReorgBudget? Find(string code)
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

    public void Save(StrictReorgBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictReorgBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict reorg budgets. Empty fields match all.</summary>
public sealed record StrictReorgBudgetQuery(string Channel, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(StrictReorgBudget item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictReorgBudget> Apply(List<StrictReorgBudget> items)
    {
        var matched = Collections.Filter(items, (StrictReorgBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict reorg budget store.</summary>
public static class StrictReorgBudgetStore
{
    public static IStrictReorgBudgetStore NewMemoryStrictReorgBudgetStore()
    {
        return new MemoryStrictReorgBudgetStore(new Dictionary<string, StrictReorgBudget>(), new List<string>());
    }

    public static IStrictReorgBudgetStore NewCachedStrictReorgBudgetStore(MemoryStrictReorgBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedStrictReorgBudgetStore(inner, StrictReorgBudgetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<StrictReorgBudget> FindStrictReorgBudgets(IStrictReorgBudgetStore store, StrictReorgBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
