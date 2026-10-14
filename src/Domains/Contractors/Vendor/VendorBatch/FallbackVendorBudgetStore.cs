namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback vendor budgets by code.</summary>
public interface IFallbackVendorBudgetStore
{
    FallbackVendorBudget? Find(string code);
    void Save(FallbackVendorBudget item);
    bool Remove(string code);
    List<FallbackVendorBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackVendorBudgetStore(Dictionary<string, FallbackVendorBudget> items, List<string> order) : IFallbackVendorBudgetStore
{
    public Dictionary<string, FallbackVendorBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackVendorBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackVendorBudget item)
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

    public List<FallbackVendorBudget> List()
    {
        var outValue = new List<FallbackVendorBudget>();
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
public sealed class CachedFallbackVendorBudgetStore(MemoryFallbackVendorBudgetStore inner, FallbackVendorBudgetCache cache, int hits, int misses) : IFallbackVendorBudgetStore
{
    public MemoryFallbackVendorBudgetStore Inner { get; set; } = inner;
    public FallbackVendorBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackVendorBudget? Find(string code)
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

    public void Save(FallbackVendorBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackVendorBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback vendor budgets. Empty fields match all.</summary>
public sealed record FallbackVendorBudgetQuery(string Channel, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackVendorBudget item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackVendorBudget> Apply(List<FallbackVendorBudget> items)
    {
        var matched = Collections.Filter(items, (FallbackVendorBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback vendor budget store.</summary>
public static class FallbackVendorBudgetStore
{
    public static IFallbackVendorBudgetStore NewMemoryFallbackVendorBudgetStore()
    {
        return new MemoryFallbackVendorBudgetStore(
            new Dictionary<string, FallbackVendorBudget>(),
            new List<string>());
    }

    public static IFallbackVendorBudgetStore NewCachedFallbackVendorBudgetStore(MemoryFallbackVendorBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackVendorBudgetStore(inner, FallbackVendorBudgetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<FallbackVendorBudget> FindFallbackVendorBudgets(IFallbackVendorBudgetStore store, FallbackVendorBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
