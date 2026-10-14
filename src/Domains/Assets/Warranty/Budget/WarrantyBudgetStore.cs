namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty budgets by code.</summary>
public interface IWarrantyBudgetStore
{
    WarrantyBudget? Find(string code);
    void Save(WarrantyBudget item);
    bool Remove(string code);
    List<WarrantyBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyBudgetStore(Dictionary<string, WarrantyBudget> items, List<string> order) : IWarrantyBudgetStore
{
    public Dictionary<string, WarrantyBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyBudget item)
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

    public List<WarrantyBudget> List()
    {
        var outValue = new List<WarrantyBudget>();
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
public sealed class CachedWarrantyBudgetStore(MemoryWarrantyBudgetStore inner, WarrantyBudgetCache cache, int hits, int misses) : IWarrantyBudgetStore
{
    public MemoryWarrantyBudgetStore Inner { get; set; } = inner;
    public WarrantyBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyBudget? Find(string code)
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

    public void Save(WarrantyBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty budgets. Empty fields match all.</summary>
public sealed record WarrantyBudgetQuery(string Source, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyBudget item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyBudget> Apply(List<WarrantyBudget> items)
    {
        var matched = Collections.Filter(items, (WarrantyBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty budget store.</summary>
public static class WarrantyBudgetStore
{
    public static IWarrantyBudgetStore NewMemoryWarrantyBudgetStore()
    {
        return new MemoryWarrantyBudgetStore(new Dictionary<string, WarrantyBudget>(), new List<string>());
    }

    public static IWarrantyBudgetStore NewCachedWarrantyBudgetStore(MemoryWarrantyBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyBudgetStore(inner, WarrantyBudgetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantyBudget> FindWarrantyBudgets(IWarrantyBudgetStore store, WarrantyBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
