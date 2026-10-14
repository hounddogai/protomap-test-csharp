namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped career path budgets by code.</summary>
public interface ICappedCareerPathBudgetStore
{
    CappedCareerPathBudget? Find(string code);
    void Save(CappedCareerPathBudget item);
    bool Remove(string code);
    List<CappedCareerPathBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedCareerPathBudgetStore(Dictionary<string, CappedCareerPathBudget> items, List<string> order) : ICappedCareerPathBudgetStore
{
    public Dictionary<string, CappedCareerPathBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedCareerPathBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedCareerPathBudget item)
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

    public List<CappedCareerPathBudget> List()
    {
        var outValue = new List<CappedCareerPathBudget>();
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
public sealed class CachedCappedCareerPathBudgetStore(MemoryCappedCareerPathBudgetStore inner, CappedCareerPathBudgetCache cache, int hits, int misses) : ICappedCareerPathBudgetStore
{
    public MemoryCappedCareerPathBudgetStore Inner { get; set; } = inner;
    public CappedCareerPathBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedCareerPathBudget? Find(string code)
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

    public void Save(CappedCareerPathBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedCareerPathBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped career path budgets. Empty fields match all.</summary>
public sealed record CappedCareerPathBudgetQuery(string Label, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedCareerPathBudget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedCareerPathBudget> Apply(List<CappedCareerPathBudget> items)
    {
        var matched = Collections.Filter(items, (CappedCareerPathBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped career path budget store.</summary>
public static class CappedCareerPathBudgetStore
{
    public static ICappedCareerPathBudgetStore NewMemoryCappedCareerPathBudgetStore()
    {
        return new MemoryCappedCareerPathBudgetStore(
            new Dictionary<string, CappedCareerPathBudget>(),
            new List<string>());
    }

    public static ICappedCareerPathBudgetStore NewCachedCappedCareerPathBudgetStore(MemoryCappedCareerPathBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedCappedCareerPathBudgetStore(
            inner,
            CappedCareerPathBudgetCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedCareerPathBudget> FindCappedCareerPathBudgets(ICappedCareerPathBudgetStore store, CappedCareerPathBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
