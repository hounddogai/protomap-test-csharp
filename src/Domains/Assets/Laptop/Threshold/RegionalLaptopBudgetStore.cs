namespace Acme.Workforce.Assets.Laptop.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional laptop budgets by code.</summary>
public interface IRegionalLaptopBudgetStore
{
    RegionalLaptopBudget? Find(string code);
    void Save(RegionalLaptopBudget item);
    bool Remove(string code);
    List<RegionalLaptopBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalLaptopBudgetStore(Dictionary<string, RegionalLaptopBudget> items, List<string> order) : IRegionalLaptopBudgetStore
{
    public Dictionary<string, RegionalLaptopBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalLaptopBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalLaptopBudget item)
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

    public List<RegionalLaptopBudget> List()
    {
        var outValue = new List<RegionalLaptopBudget>();
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
public sealed class CachedRegionalLaptopBudgetStore(MemoryRegionalLaptopBudgetStore inner, RegionalLaptopBudgetCache cache, int hits, int misses) : IRegionalLaptopBudgetStore
{
    public MemoryRegionalLaptopBudgetStore Inner { get; set; } = inner;
    public RegionalLaptopBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalLaptopBudget? Find(string code)
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

    public void Save(RegionalLaptopBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalLaptopBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional laptop budgets. Empty fields match all.</summary>
public sealed record RegionalLaptopBudgetQuery(string Region, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalLaptopBudget item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalLaptopBudget> Apply(List<RegionalLaptopBudget> items)
    {
        var matched = Collections.Filter(items, (RegionalLaptopBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional laptop budget store.</summary>
public static class RegionalLaptopBudgetStore
{
    public static IRegionalLaptopBudgetStore NewMemoryRegionalLaptopBudgetStore()
    {
        return new MemoryRegionalLaptopBudgetStore(
            new Dictionary<string, RegionalLaptopBudget>(),
            new List<string>());
    }

    public static IRegionalLaptopBudgetStore NewCachedRegionalLaptopBudgetStore(MemoryRegionalLaptopBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalLaptopBudgetStore(inner, RegionalLaptopBudgetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RegionalLaptopBudget> FindRegionalLaptopBudgets(IRegionalLaptopBudgetStore store, RegionalLaptopBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
