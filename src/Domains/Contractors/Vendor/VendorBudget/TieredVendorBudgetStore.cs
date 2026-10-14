namespace Acme.Workforce.Contractors.Vendor.VendorBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered vendor budgets by code.</summary>
public interface ITieredVendorBudgetStore
{
    TieredVendorBudget? Find(string code);
    void Save(TieredVendorBudget item);
    bool Remove(string code);
    List<TieredVendorBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredVendorBudgetStore(Dictionary<string, TieredVendorBudget> items, List<string> order) : ITieredVendorBudgetStore
{
    public Dictionary<string, TieredVendorBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredVendorBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredVendorBudget item)
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

    public List<TieredVendorBudget> List()
    {
        var outValue = new List<TieredVendorBudget>();
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
public sealed class CachedTieredVendorBudgetStore(MemoryTieredVendorBudgetStore inner, TieredVendorBudgetCache cache, int hits, int misses) : ITieredVendorBudgetStore
{
    public MemoryTieredVendorBudgetStore Inner { get; set; } = inner;
    public TieredVendorBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredVendorBudget? Find(string code)
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

    public void Save(TieredVendorBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredVendorBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered vendor budgets. Empty fields match all.</summary>
public sealed record TieredVendorBudgetQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(TieredVendorBudget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredVendorBudget> Apply(List<TieredVendorBudget> items)
    {
        var matched = Collections.Filter(items, (TieredVendorBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered vendor budget store.</summary>
public static class TieredVendorBudgetStore
{
    public static ITieredVendorBudgetStore NewMemoryTieredVendorBudgetStore()
    {
        return new MemoryTieredVendorBudgetStore(new Dictionary<string, TieredVendorBudget>(), new List<string>());
    }

    public static ITieredVendorBudgetStore NewCachedTieredVendorBudgetStore(MemoryTieredVendorBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedTieredVendorBudgetStore(inner, TieredVendorBudgetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TieredVendorBudget> FindTieredVendorBudgets(ITieredVendorBudgetStore store, TieredVendorBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
