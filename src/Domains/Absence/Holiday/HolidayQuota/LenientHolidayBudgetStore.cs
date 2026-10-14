namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient holiday budgets by code.</summary>
public interface ILenientHolidayBudgetStore
{
    LenientHolidayBudget? Find(string code);
    void Save(LenientHolidayBudget item);
    bool Remove(string code);
    List<LenientHolidayBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientHolidayBudgetStore(Dictionary<string, LenientHolidayBudget> items, List<string> order) : ILenientHolidayBudgetStore
{
    public Dictionary<string, LenientHolidayBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientHolidayBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientHolidayBudget item)
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

    public List<LenientHolidayBudget> List()
    {
        var outValue = new List<LenientHolidayBudget>();
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
public sealed class CachedLenientHolidayBudgetStore(MemoryLenientHolidayBudgetStore inner, LenientHolidayBudgetCache cache, int hits, int misses) : ILenientHolidayBudgetStore
{
    public MemoryLenientHolidayBudgetStore Inner { get; set; } = inner;
    public LenientHolidayBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientHolidayBudget? Find(string code)
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

    public void Save(LenientHolidayBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientHolidayBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient holiday budgets. Empty fields match all.</summary>
public sealed record LenientHolidayBudgetQuery(string Unit, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(LenientHolidayBudget item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientHolidayBudget> Apply(List<LenientHolidayBudget> items)
    {
        var matched = Collections.Filter(items, (LenientHolidayBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient holiday budget store.</summary>
public static class LenientHolidayBudgetStore
{
    public static ILenientHolidayBudgetStore NewMemoryLenientHolidayBudgetStore()
    {
        return new MemoryLenientHolidayBudgetStore(
            new Dictionary<string, LenientHolidayBudget>(),
            new List<string>());
    }

    public static ILenientHolidayBudgetStore NewCachedLenientHolidayBudgetStore(MemoryLenientHolidayBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedLenientHolidayBudgetStore(inner, LenientHolidayBudgetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LenientHolidayBudget> FindLenientHolidayBudgets(ILenientHolidayBudgetStore store, LenientHolidayBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
