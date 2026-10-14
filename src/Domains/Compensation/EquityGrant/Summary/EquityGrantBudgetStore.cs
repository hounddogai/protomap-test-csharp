namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant budgets by code.</summary>
public interface IEquityGrantBudgetStore
{
    EquityGrantBudget? Find(string code);
    void Save(EquityGrantBudget item);
    bool Remove(string code);
    List<EquityGrantBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantBudgetStore(Dictionary<string, EquityGrantBudget> items, List<string> order) : IEquityGrantBudgetStore
{
    public Dictionary<string, EquityGrantBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantBudget item)
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

    public List<EquityGrantBudget> List()
    {
        var outValue = new List<EquityGrantBudget>();
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
public sealed class CachedEquityGrantBudgetStore(MemoryEquityGrantBudgetStore inner, EquityGrantBudgetCache cache, int hits, int misses) : IEquityGrantBudgetStore
{
    public MemoryEquityGrantBudgetStore Inner { get; set; } = inner;
    public EquityGrantBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantBudget? Find(string code)
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

    public void Save(EquityGrantBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant budgets. Empty fields match all.</summary>
public sealed record EquityGrantBudgetQuery(string Channel, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantBudget item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantBudget> Apply(List<EquityGrantBudget> items)
    {
        var matched = Collections.Filter(items, (EquityGrantBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant budget store.</summary>
public static class EquityGrantBudgetStore
{
    public static IEquityGrantBudgetStore NewMemoryEquityGrantBudgetStore()
    {
        return new MemoryEquityGrantBudgetStore(new Dictionary<string, EquityGrantBudget>(), new List<string>());
    }

    public static IEquityGrantBudgetStore NewCachedEquityGrantBudgetStore(MemoryEquityGrantBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantBudgetStore(inner, EquityGrantBudgetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<EquityGrantBudget> FindEquityGrantBudgets(IEquityGrantBudgetStore store, EquityGrantBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
