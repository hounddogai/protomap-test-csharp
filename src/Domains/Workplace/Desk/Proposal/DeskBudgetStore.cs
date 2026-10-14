namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk budgets by code.</summary>
public interface IDeskBudgetStore
{
    DeskBudget? Find(string code);
    void Save(DeskBudget item);
    bool Remove(string code);
    List<DeskBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskBudgetStore(Dictionary<string, DeskBudget> items, List<string> order) : IDeskBudgetStore
{
    public Dictionary<string, DeskBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskBudget item)
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

    public List<DeskBudget> List()
    {
        var outValue = new List<DeskBudget>();
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
public sealed class CachedDeskBudgetStore(MemoryDeskBudgetStore inner, DeskBudgetCache cache, int hits, int misses) : IDeskBudgetStore
{
    public MemoryDeskBudgetStore Inner { get; set; } = inner;
    public DeskBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskBudget? Find(string code)
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

    public void Save(DeskBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk budgets. Empty fields match all.</summary>
public sealed record DeskBudgetQuery(string Channel, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeskBudget item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskBudget> Apply(List<DeskBudget> items)
    {
        var matched = Collections.Filter(items, (DeskBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk budget store.</summary>
public static class DeskBudgetStore
{
    public static IDeskBudgetStore NewMemoryDeskBudgetStore()
    {
        return new MemoryDeskBudgetStore(new Dictionary<string, DeskBudget>(), new List<string>());
    }

    public static IDeskBudgetStore NewCachedDeskBudgetStore(MemoryDeskBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedDeskBudgetStore(inner, DeskBudgetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeskBudget> FindDeskBudgets(IDeskBudgetStore store, DeskBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
