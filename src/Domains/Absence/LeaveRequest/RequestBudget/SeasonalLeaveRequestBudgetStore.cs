namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal leave request budgets by code.</summary>
public interface ISeasonalLeaveRequestBudgetStore
{
    SeasonalLeaveRequestBudget? Find(string code);
    void Save(SeasonalLeaveRequestBudget item);
    bool Remove(string code);
    List<SeasonalLeaveRequestBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalLeaveRequestBudgetStore(Dictionary<string, SeasonalLeaveRequestBudget> items, List<string> order) : ISeasonalLeaveRequestBudgetStore
{
    public Dictionary<string, SeasonalLeaveRequestBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalLeaveRequestBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalLeaveRequestBudget item)
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

    public List<SeasonalLeaveRequestBudget> List()
    {
        var outValue = new List<SeasonalLeaveRequestBudget>();
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
public sealed class CachedSeasonalLeaveRequestBudgetStore(MemorySeasonalLeaveRequestBudgetStore inner, SeasonalLeaveRequestBudgetCache cache, int hits, int misses) : ISeasonalLeaveRequestBudgetStore
{
    public MemorySeasonalLeaveRequestBudgetStore Inner { get; set; } = inner;
    public SeasonalLeaveRequestBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalLeaveRequestBudget? Find(string code)
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

    public void Save(SeasonalLeaveRequestBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalLeaveRequestBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal leave request budgets. Empty fields match all.</summary>
public sealed record SeasonalLeaveRequestBudgetQuery(string Category, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalLeaveRequestBudget item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalLeaveRequestBudget> Apply(List<SeasonalLeaveRequestBudget> items)
    {
        var matched = Collections.Filter(items, (SeasonalLeaveRequestBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal leave request budget store.</summary>
public static class SeasonalLeaveRequestBudgetStore
{
    public static ISeasonalLeaveRequestBudgetStore NewMemorySeasonalLeaveRequestBudgetStore()
    {
        return new MemorySeasonalLeaveRequestBudgetStore(
            new Dictionary<string, SeasonalLeaveRequestBudget>(),
            new List<string>());
    }

    public static ISeasonalLeaveRequestBudgetStore NewCachedSeasonalLeaveRequestBudgetStore(MemorySeasonalLeaveRequestBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalLeaveRequestBudgetStore(
            inner,
            SeasonalLeaveRequestBudgetCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalLeaveRequestBudget> FindSeasonalLeaveRequestBudgets(ISeasonalLeaveRequestBudgetStore store, SeasonalLeaveRequestBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
