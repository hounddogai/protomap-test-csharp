namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional headcount budgets by code.</summary>
public interface IRegionalHeadcountBudgetStore
{
    RegionalHeadcountBudget? Find(string code);
    void Save(RegionalHeadcountBudget item);
    bool Remove(string code);
    List<RegionalHeadcountBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalHeadcountBudgetStore(Dictionary<string, RegionalHeadcountBudget> items, List<string> order) : IRegionalHeadcountBudgetStore
{
    public Dictionary<string, RegionalHeadcountBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalHeadcountBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalHeadcountBudget item)
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

    public List<RegionalHeadcountBudget> List()
    {
        var outValue = new List<RegionalHeadcountBudget>();
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
public sealed class CachedRegionalHeadcountBudgetStore(MemoryRegionalHeadcountBudgetStore inner, RegionalHeadcountBudgetCache cache, int hits, int misses) : IRegionalHeadcountBudgetStore
{
    public MemoryRegionalHeadcountBudgetStore Inner { get; set; } = inner;
    public RegionalHeadcountBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalHeadcountBudget? Find(string code)
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

    public void Save(RegionalHeadcountBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalHeadcountBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional headcount budgets. Empty fields match all.</summary>
public sealed record RegionalHeadcountBudgetQuery(string Segment, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalHeadcountBudget item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<RegionalHeadcountBudget> Apply(List<RegionalHeadcountBudget> items)
    {
        var matched = Collections.Filter(items, (RegionalHeadcountBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional headcount budget store.</summary>
public static class RegionalHeadcountBudgetStore
{
    public static IRegionalHeadcountBudgetStore NewMemoryRegionalHeadcountBudgetStore()
    {
        return new MemoryRegionalHeadcountBudgetStore(
            new Dictionary<string, RegionalHeadcountBudget>(),
            new List<string>());
    }

    public static IRegionalHeadcountBudgetStore NewCachedRegionalHeadcountBudgetStore(MemoryRegionalHeadcountBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalHeadcountBudgetStore(
            inner,
            RegionalHeadcountBudgetCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RegionalHeadcountBudget> FindRegionalHeadcountBudgets(IRegionalHeadcountBudgetStore store, RegionalHeadcountBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
