namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge budgets by code.</summary>
public interface IAccessBadgeBudgetStore
{
    AccessBadgeBudget? Find(string code);
    void Save(AccessBadgeBudget item);
    bool Remove(string code);
    List<AccessBadgeBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeBudgetStore(Dictionary<string, AccessBadgeBudget> items, List<string> order) : IAccessBadgeBudgetStore
{
    public Dictionary<string, AccessBadgeBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeBudget item)
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

    public List<AccessBadgeBudget> List()
    {
        var outValue = new List<AccessBadgeBudget>();
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
public sealed class CachedAccessBadgeBudgetStore(MemoryAccessBadgeBudgetStore inner, AccessBadgeBudgetCache cache, int hits, int misses) : IAccessBadgeBudgetStore
{
    public MemoryAccessBadgeBudgetStore Inner { get; set; } = inner;
    public AccessBadgeBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeBudget? Find(string code)
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

    public void Save(AccessBadgeBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge budgets. Empty fields match all.</summary>
public sealed record AccessBadgeBudgetQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeBudget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeBudget> Apply(List<AccessBadgeBudget> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge budget store.</summary>
public static class AccessBadgeBudgetStore
{
    public static IAccessBadgeBudgetStore NewMemoryAccessBadgeBudgetStore()
    {
        return new MemoryAccessBadgeBudgetStore(new Dictionary<string, AccessBadgeBudget>(), new List<string>());
    }

    public static IAccessBadgeBudgetStore NewCachedAccessBadgeBudgetStore(MemoryAccessBadgeBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeBudgetStore(inner, AccessBadgeBudgetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeBudget> FindAccessBadgeBudgets(IAccessBadgeBudgetStore store, AccessBadgeBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
