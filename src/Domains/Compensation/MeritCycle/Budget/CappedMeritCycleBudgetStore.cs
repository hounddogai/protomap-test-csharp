namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped merit cycle budgets by code.</summary>
public interface ICappedMeritCycleBudgetStore
{
    CappedMeritCycleBudget? Find(string code);
    void Save(CappedMeritCycleBudget item);
    bool Remove(string code);
    List<CappedMeritCycleBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedMeritCycleBudgetStore(Dictionary<string, CappedMeritCycleBudget> items, List<string> order) : ICappedMeritCycleBudgetStore
{
    public Dictionary<string, CappedMeritCycleBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedMeritCycleBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedMeritCycleBudget item)
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

    public List<CappedMeritCycleBudget> List()
    {
        var outValue = new List<CappedMeritCycleBudget>();
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
public sealed class CachedCappedMeritCycleBudgetStore(MemoryCappedMeritCycleBudgetStore inner, CappedMeritCycleBudgetCache cache, int hits, int misses) : ICappedMeritCycleBudgetStore
{
    public MemoryCappedMeritCycleBudgetStore Inner { get; set; } = inner;
    public CappedMeritCycleBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedMeritCycleBudget? Find(string code)
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

    public void Save(CappedMeritCycleBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedMeritCycleBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped merit cycle budgets. Empty fields match all.</summary>
public sealed record CappedMeritCycleBudgetQuery(string Team, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedMeritCycleBudget item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedMeritCycleBudget> Apply(List<CappedMeritCycleBudget> items)
    {
        var matched = Collections.Filter(items, (CappedMeritCycleBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped merit cycle budget store.</summary>
public static class CappedMeritCycleBudgetStore
{
    public static ICappedMeritCycleBudgetStore NewMemoryCappedMeritCycleBudgetStore()
    {
        return new MemoryCappedMeritCycleBudgetStore(
            new Dictionary<string, CappedMeritCycleBudget>(),
            new List<string>());
    }

    public static ICappedMeritCycleBudgetStore NewCachedCappedMeritCycleBudgetStore(MemoryCappedMeritCycleBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedCappedMeritCycleBudgetStore(
            inner,
            CappedMeritCycleBudgetCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedMeritCycleBudget> FindCappedMeritCycleBudgets(ICappedMeritCycleBudgetStore store, CappedMeritCycleBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
