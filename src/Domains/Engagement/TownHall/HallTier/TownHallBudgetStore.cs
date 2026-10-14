namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall budgets by code.</summary>
public interface ITownHallBudgetStore
{
    TownHallBudget? Find(string code);
    void Save(TownHallBudget item);
    bool Remove(string code);
    List<TownHallBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallBudgetStore(Dictionary<string, TownHallBudget> items, List<string> order) : ITownHallBudgetStore
{
    public Dictionary<string, TownHallBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallBudget item)
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

    public List<TownHallBudget> List()
    {
        var outValue = new List<TownHallBudget>();
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
public sealed class CachedTownHallBudgetStore(MemoryTownHallBudgetStore inner, TownHallBudgetCache cache, int hits, int misses) : ITownHallBudgetStore
{
    public MemoryTownHallBudgetStore Inner { get; set; } = inner;
    public TownHallBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallBudget? Find(string code)
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

    public void Save(TownHallBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall budgets. Empty fields match all.</summary>
public sealed record TownHallBudgetQuery(string Name, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallBudget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TownHallBudget> Apply(List<TownHallBudget> items)
    {
        var matched = Collections.Filter(items, (TownHallBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall budget store.</summary>
public static class TownHallBudgetStore
{
    public static ITownHallBudgetStore NewMemoryTownHallBudgetStore()
    {
        return new MemoryTownHallBudgetStore(new Dictionary<string, TownHallBudget>(), new List<string>());
    }

    public static ITownHallBudgetStore NewCachedTownHallBudgetStore(MemoryTownHallBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallBudgetStore(inner, TownHallBudgetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TownHallBudget> FindTownHallBudgets(ITownHallBudgetStore store, TownHallBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
