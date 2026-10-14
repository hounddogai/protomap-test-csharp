namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary leave request budgets by code.</summary>
public interface IPrimaryLeaveRequestBudgetStore
{
    PrimaryLeaveRequestBudget? Find(string code);
    void Save(PrimaryLeaveRequestBudget item);
    bool Remove(string code);
    List<PrimaryLeaveRequestBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryLeaveRequestBudgetStore(Dictionary<string, PrimaryLeaveRequestBudget> items, List<string> order) : IPrimaryLeaveRequestBudgetStore
{
    public Dictionary<string, PrimaryLeaveRequestBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryLeaveRequestBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryLeaveRequestBudget item)
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

    public List<PrimaryLeaveRequestBudget> List()
    {
        var outValue = new List<PrimaryLeaveRequestBudget>();
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
public sealed class CachedPrimaryLeaveRequestBudgetStore(MemoryPrimaryLeaveRequestBudgetStore inner, PrimaryLeaveRequestBudgetCache cache, int hits, int misses) : IPrimaryLeaveRequestBudgetStore
{
    public MemoryPrimaryLeaveRequestBudgetStore Inner { get; set; } = inner;
    public PrimaryLeaveRequestBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryLeaveRequestBudget? Find(string code)
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

    public void Save(PrimaryLeaveRequestBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryLeaveRequestBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary leave request budgets. Empty fields match all.</summary>
public sealed record PrimaryLeaveRequestBudgetQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryLeaveRequestBudget item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryLeaveRequestBudget> Apply(List<PrimaryLeaveRequestBudget> items)
    {
        var matched = Collections.Filter(items, (PrimaryLeaveRequestBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary leave request budget store.</summary>
public static class PrimaryLeaveRequestBudgetStore
{
    public static IPrimaryLeaveRequestBudgetStore NewMemoryPrimaryLeaveRequestBudgetStore()
    {
        return new MemoryPrimaryLeaveRequestBudgetStore(
            new Dictionary<string, PrimaryLeaveRequestBudget>(),
            new List<string>());
    }

    public static IPrimaryLeaveRequestBudgetStore NewCachedPrimaryLeaveRequestBudgetStore(MemoryPrimaryLeaveRequestBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryLeaveRequestBudgetStore(
            inner,
            PrimaryLeaveRequestBudgetCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryLeaveRequestBudget> FindPrimaryLeaveRequestBudgets(IPrimaryLeaveRequestBudgetStore store, PrimaryLeaveRequestBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
