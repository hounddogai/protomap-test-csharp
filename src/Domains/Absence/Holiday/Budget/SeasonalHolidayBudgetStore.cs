namespace Acme.Workforce.Absence.Holiday.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal holiday budgets by code.</summary>
public interface ISeasonalHolidayBudgetStore
{
    SeasonalHolidayBudget? Find(string code);
    void Save(SeasonalHolidayBudget item);
    bool Remove(string code);
    List<SeasonalHolidayBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalHolidayBudgetStore(Dictionary<string, SeasonalHolidayBudget> items, List<string> order) : ISeasonalHolidayBudgetStore
{
    public Dictionary<string, SeasonalHolidayBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalHolidayBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalHolidayBudget item)
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

    public List<SeasonalHolidayBudget> List()
    {
        var outValue = new List<SeasonalHolidayBudget>();
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
public sealed class CachedSeasonalHolidayBudgetStore(MemorySeasonalHolidayBudgetStore inner, SeasonalHolidayBudgetCache cache, int hits, int misses) : ISeasonalHolidayBudgetStore
{
    public MemorySeasonalHolidayBudgetStore Inner { get; set; } = inner;
    public SeasonalHolidayBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalHolidayBudget? Find(string code)
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

    public void Save(SeasonalHolidayBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalHolidayBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal holiday budgets. Empty fields match all.</summary>
public sealed record SeasonalHolidayBudgetQuery(string Source, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalHolidayBudget item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<SeasonalHolidayBudget> Apply(List<SeasonalHolidayBudget> items)
    {
        var matched = Collections.Filter(items, (SeasonalHolidayBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal holiday budget store.</summary>
public static class SeasonalHolidayBudgetStore
{
    public static ISeasonalHolidayBudgetStore NewMemorySeasonalHolidayBudgetStore()
    {
        return new MemorySeasonalHolidayBudgetStore(
            new Dictionary<string, SeasonalHolidayBudget>(),
            new List<string>());
    }

    public static ISeasonalHolidayBudgetStore NewCachedSeasonalHolidayBudgetStore(MemorySeasonalHolidayBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalHolidayBudgetStore(inner, SeasonalHolidayBudgetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SeasonalHolidayBudget> FindSeasonalHolidayBudgets(ISeasonalHolidayBudgetStore store, SeasonalHolidayBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
