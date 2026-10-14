namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form allocations by code.</summary>
public interface ITaxFormAllocationStore
{
    TaxFormAllocation? Find(string code);
    void Save(TaxFormAllocation item);
    bool Remove(string code);
    List<TaxFormAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormAllocationStore(Dictionary<string, TaxFormAllocation> items, List<string> order) : ITaxFormAllocationStore
{
    public Dictionary<string, TaxFormAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormAllocation item)
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

    public List<TaxFormAllocation> List()
    {
        var outValue = new List<TaxFormAllocation>();
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
public sealed class CachedTaxFormAllocationStore(MemoryTaxFormAllocationStore inner, TaxFormAllocationCache cache, int hits, int misses) : ITaxFormAllocationStore
{
    public MemoryTaxFormAllocationStore Inner { get; set; } = inner;
    public TaxFormAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormAllocation? Find(string code)
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

    public void Save(TaxFormAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form allocations. Empty fields match all.</summary>
public sealed record TaxFormAllocationQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormAllocation item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormAllocation> Apply(List<TaxFormAllocation> items)
    {
        var matched = Collections.Filter(items, (TaxFormAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form allocation store.</summary>
public static class TaxFormAllocationStore
{
    public static ITaxFormAllocationStore NewMemoryTaxFormAllocationStore()
    {
        return new MemoryTaxFormAllocationStore(new Dictionary<string, TaxFormAllocation>(), new List<string>());
    }

    public static ITaxFormAllocationStore NewCachedTaxFormAllocationStore(MemoryTaxFormAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormAllocationStore(inner, TaxFormAllocationCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<TaxFormAllocation> FindTaxFormAllocations(ITaxFormAllocationStore store, TaxFormAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
