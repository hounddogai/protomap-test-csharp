namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive reorg budgets by code.</summary>
public interface IAdaptiveReorgBudgetStore
{
    AdaptiveReorgBudget? Find(string code);
    void Save(AdaptiveReorgBudget item);
    bool Remove(string code);
    List<AdaptiveReorgBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveReorgBudgetStore(Dictionary<string, AdaptiveReorgBudget> items, List<string> order) : IAdaptiveReorgBudgetStore
{
    public Dictionary<string, AdaptiveReorgBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveReorgBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveReorgBudget item)
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

    public List<AdaptiveReorgBudget> List()
    {
        var outValue = new List<AdaptiveReorgBudget>();
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
public sealed class CachedAdaptiveReorgBudgetStore(MemoryAdaptiveReorgBudgetStore inner, AdaptiveReorgBudgetCache cache, int hits, int misses) : IAdaptiveReorgBudgetStore
{
    public MemoryAdaptiveReorgBudgetStore Inner { get; set; } = inner;
    public AdaptiveReorgBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveReorgBudget? Find(string code)
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

    public void Save(AdaptiveReorgBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveReorgBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive reorg budgets. Empty fields match all.</summary>
public sealed record AdaptiveReorgBudgetQuery(string Category, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveReorgBudget item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<AdaptiveReorgBudget> Apply(List<AdaptiveReorgBudget> items)
    {
        var matched = Collections.Filter(items, (AdaptiveReorgBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive reorg budget store.</summary>
public static class AdaptiveReorgBudgetStore
{
    public static IAdaptiveReorgBudgetStore NewMemoryAdaptiveReorgBudgetStore()
    {
        return new MemoryAdaptiveReorgBudgetStore(new Dictionary<string, AdaptiveReorgBudget>(), new List<string>());
    }

    public static IAdaptiveReorgBudgetStore NewCachedAdaptiveReorgBudgetStore(MemoryAdaptiveReorgBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveReorgBudgetStore(inner, AdaptiveReorgBudgetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AdaptiveReorgBudget> FindAdaptiveReorgBudgets(IAdaptiveReorgBudgetStore store, AdaptiveReorgBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
