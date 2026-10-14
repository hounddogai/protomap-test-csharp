namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy headcount budgets by code.</summary>
public interface ILegacyHeadcountBudgetStore
{
    LegacyHeadcountBudget? Find(string code);
    void Save(LegacyHeadcountBudget item);
    bool Remove(string code);
    List<LegacyHeadcountBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyHeadcountBudgetStore(Dictionary<string, LegacyHeadcountBudget> items, List<string> order) : ILegacyHeadcountBudgetStore
{
    public Dictionary<string, LegacyHeadcountBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyHeadcountBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyHeadcountBudget item)
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

    public List<LegacyHeadcountBudget> List()
    {
        var outValue = new List<LegacyHeadcountBudget>();
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
public sealed class CachedLegacyHeadcountBudgetStore(MemoryLegacyHeadcountBudgetStore inner, LegacyHeadcountBudgetCache cache, int hits, int misses) : ILegacyHeadcountBudgetStore
{
    public MemoryLegacyHeadcountBudgetStore Inner { get; set; } = inner;
    public LegacyHeadcountBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyHeadcountBudget? Find(string code)
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

    public void Save(LegacyHeadcountBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyHeadcountBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy headcount budgets. Empty fields match all.</summary>
public sealed record LegacyHeadcountBudgetQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyHeadcountBudget item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyHeadcountBudget> Apply(List<LegacyHeadcountBudget> items)
    {
        var matched = Collections.Filter(items, (LegacyHeadcountBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy headcount budget store.</summary>
public static class LegacyHeadcountBudgetStore
{
    public static ILegacyHeadcountBudgetStore NewMemoryLegacyHeadcountBudgetStore()
    {
        return new MemoryLegacyHeadcountBudgetStore(
            new Dictionary<string, LegacyHeadcountBudget>(),
            new List<string>());
    }

    public static ILegacyHeadcountBudgetStore NewCachedLegacyHeadcountBudgetStore(MemoryLegacyHeadcountBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyHeadcountBudgetStore(inner, LegacyHeadcountBudgetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LegacyHeadcountBudget> FindLegacyHeadcountBudgets(ILegacyHeadcountBudgetStore store, LegacyHeadcountBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
