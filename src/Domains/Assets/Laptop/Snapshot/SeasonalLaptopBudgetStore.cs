namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal laptop budgets by code.</summary>
public interface ISeasonalLaptopBudgetStore
{
    SeasonalLaptopBudget? Find(string code);
    void Save(SeasonalLaptopBudget item);
    bool Remove(string code);
    List<SeasonalLaptopBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalLaptopBudgetStore(Dictionary<string, SeasonalLaptopBudget> items, List<string> order) : ISeasonalLaptopBudgetStore
{
    public Dictionary<string, SeasonalLaptopBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalLaptopBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalLaptopBudget item)
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

    public List<SeasonalLaptopBudget> List()
    {
        var outValue = new List<SeasonalLaptopBudget>();
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
public sealed class CachedSeasonalLaptopBudgetStore(MemorySeasonalLaptopBudgetStore inner, SeasonalLaptopBudgetCache cache, int hits, int misses) : ISeasonalLaptopBudgetStore
{
    public MemorySeasonalLaptopBudgetStore Inner { get; set; } = inner;
    public SeasonalLaptopBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalLaptopBudget? Find(string code)
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

    public void Save(SeasonalLaptopBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalLaptopBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal laptop budgets. Empty fields match all.</summary>
public sealed record SeasonalLaptopBudgetQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalLaptopBudget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalLaptopBudget> Apply(List<SeasonalLaptopBudget> items)
    {
        var matched = Collections.Filter(items, (SeasonalLaptopBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal laptop budget store.</summary>
public static class SeasonalLaptopBudgetStore
{
    public static ISeasonalLaptopBudgetStore NewMemorySeasonalLaptopBudgetStore()
    {
        return new MemorySeasonalLaptopBudgetStore(
            new Dictionary<string, SeasonalLaptopBudget>(),
            new List<string>());
    }

    public static ISeasonalLaptopBudgetStore NewCachedSeasonalLaptopBudgetStore(MemorySeasonalLaptopBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalLaptopBudgetStore(inner, SeasonalLaptopBudgetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SeasonalLaptopBudget> FindSeasonalLaptopBudgets(ISeasonalLaptopBudgetStore store, SeasonalLaptopBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
