namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal succession plan budgets by code.</summary>
public interface ISeasonalSuccessionPlanBudgetStore
{
    SeasonalSuccessionPlanBudget? Find(string code);
    void Save(SeasonalSuccessionPlanBudget item);
    bool Remove(string code);
    List<SeasonalSuccessionPlanBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalSuccessionPlanBudgetStore(Dictionary<string, SeasonalSuccessionPlanBudget> items, List<string> order) : ISeasonalSuccessionPlanBudgetStore
{
    public Dictionary<string, SeasonalSuccessionPlanBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalSuccessionPlanBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalSuccessionPlanBudget item)
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

    public List<SeasonalSuccessionPlanBudget> List()
    {
        var outValue = new List<SeasonalSuccessionPlanBudget>();
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
public sealed class CachedSeasonalSuccessionPlanBudgetStore(MemorySeasonalSuccessionPlanBudgetStore inner, SeasonalSuccessionPlanBudgetCache cache, int hits, int misses) : ISeasonalSuccessionPlanBudgetStore
{
    public MemorySeasonalSuccessionPlanBudgetStore Inner { get; set; } = inner;
    public SeasonalSuccessionPlanBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalSuccessionPlanBudget? Find(string code)
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

    public void Save(SeasonalSuccessionPlanBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalSuccessionPlanBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal succession plan budgets. Empty fields match all.</summary>
public sealed record SeasonalSuccessionPlanBudgetQuery(string Source, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalSuccessionPlanBudget item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalSuccessionPlanBudget> Apply(List<SeasonalSuccessionPlanBudget> items)
    {
        var matched = Collections.Filter(items, (SeasonalSuccessionPlanBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal succession plan budget store.</summary>
public static class SeasonalSuccessionPlanBudgetStore
{
    public static ISeasonalSuccessionPlanBudgetStore NewMemorySeasonalSuccessionPlanBudgetStore()
    {
        return new MemorySeasonalSuccessionPlanBudgetStore(
            new Dictionary<string, SeasonalSuccessionPlanBudget>(),
            new List<string>());
    }

    public static ISeasonalSuccessionPlanBudgetStore NewCachedSeasonalSuccessionPlanBudgetStore(MemorySeasonalSuccessionPlanBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalSuccessionPlanBudgetStore(
            inner,
            SeasonalSuccessionPlanBudgetCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalSuccessionPlanBudget> FindSeasonalSuccessionPlanBudgets(ISeasonalSuccessionPlanBudgetStore store, SeasonalSuccessionPlanBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
