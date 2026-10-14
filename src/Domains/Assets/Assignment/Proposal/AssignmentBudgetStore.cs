namespace Acme.Workforce.Assets.Assignment.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment budgets by code.</summary>
public interface IAssignmentBudgetStore
{
    AssignmentBudget? Find(string code);
    void Save(AssignmentBudget item);
    bool Remove(string code);
    List<AssignmentBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentBudgetStore(Dictionary<string, AssignmentBudget> items, List<string> order) : IAssignmentBudgetStore
{
    public Dictionary<string, AssignmentBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentBudget item)
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

    public List<AssignmentBudget> List()
    {
        var outValue = new List<AssignmentBudget>();
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
public sealed class CachedAssignmentBudgetStore(MemoryAssignmentBudgetStore inner, AssignmentBudgetCache cache, int hits, int misses) : IAssignmentBudgetStore
{
    public MemoryAssignmentBudgetStore Inner { get; set; } = inner;
    public AssignmentBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentBudget? Find(string code)
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

    public void Save(AssignmentBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment budgets. Empty fields match all.</summary>
public sealed record AssignmentBudgetQuery(string Source, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentBudget item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<AssignmentBudget> Apply(List<AssignmentBudget> items)
    {
        var matched = Collections.Filter(items, (AssignmentBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment budget store.</summary>
public static class AssignmentBudgetStore
{
    public static IAssignmentBudgetStore NewMemoryAssignmentBudgetStore()
    {
        return new MemoryAssignmentBudgetStore(new Dictionary<string, AssignmentBudget>(), new List<string>());
    }

    public static IAssignmentBudgetStore NewCachedAssignmentBudgetStore(MemoryAssignmentBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentBudgetStore(inner, AssignmentBudgetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssignmentBudget> FindAssignmentBudgets(IAssignmentBudgetStore store, AssignmentBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
