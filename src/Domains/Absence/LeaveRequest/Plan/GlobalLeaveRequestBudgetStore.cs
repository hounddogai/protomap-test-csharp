namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global leave request budgets by code.</summary>
public interface IGlobalLeaveRequestBudgetStore
{
    GlobalLeaveRequestBudget? Find(string code);
    void Save(GlobalLeaveRequestBudget item);
    bool Remove(string code);
    List<GlobalLeaveRequestBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalLeaveRequestBudgetStore(Dictionary<string, GlobalLeaveRequestBudget> items, List<string> order) : IGlobalLeaveRequestBudgetStore
{
    public Dictionary<string, GlobalLeaveRequestBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalLeaveRequestBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalLeaveRequestBudget item)
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

    public List<GlobalLeaveRequestBudget> List()
    {
        var outValue = new List<GlobalLeaveRequestBudget>();
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
public sealed class CachedGlobalLeaveRequestBudgetStore(MemoryGlobalLeaveRequestBudgetStore inner, GlobalLeaveRequestBudgetCache cache, int hits, int misses) : IGlobalLeaveRequestBudgetStore
{
    public MemoryGlobalLeaveRequestBudgetStore Inner { get; set; } = inner;
    public GlobalLeaveRequestBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalLeaveRequestBudget? Find(string code)
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

    public void Save(GlobalLeaveRequestBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalLeaveRequestBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global leave request budgets. Empty fields match all.</summary>
public sealed record GlobalLeaveRequestBudgetQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalLeaveRequestBudget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalLeaveRequestBudget> Apply(List<GlobalLeaveRequestBudget> items)
    {
        var matched = Collections.Filter(items, (GlobalLeaveRequestBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global leave request budget store.</summary>
public static class GlobalLeaveRequestBudgetStore
{
    public static IGlobalLeaveRequestBudgetStore NewMemoryGlobalLeaveRequestBudgetStore()
    {
        return new MemoryGlobalLeaveRequestBudgetStore(
            new Dictionary<string, GlobalLeaveRequestBudget>(),
            new List<string>());
    }

    public static IGlobalLeaveRequestBudgetStore NewCachedGlobalLeaveRequestBudgetStore(MemoryGlobalLeaveRequestBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalLeaveRequestBudgetStore(
            inner,
            GlobalLeaveRequestBudgetCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<GlobalLeaveRequestBudget> FindGlobalLeaveRequestBudgets(IGlobalLeaveRequestBudgetStore store, GlobalLeaveRequestBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
