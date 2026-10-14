namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction allocations by code.</summary>
public interface IDeductionAllocationStore
{
    DeductionAllocation? Find(string code);
    void Save(DeductionAllocation item);
    bool Remove(string code);
    List<DeductionAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionAllocationStore(Dictionary<string, DeductionAllocation> items, List<string> order) : IDeductionAllocationStore
{
    public Dictionary<string, DeductionAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionAllocation item)
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

    public List<DeductionAllocation> List()
    {
        var outValue = new List<DeductionAllocation>();
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
public sealed class CachedDeductionAllocationStore(MemoryDeductionAllocationStore inner, DeductionAllocationCache cache, int hits, int misses) : IDeductionAllocationStore
{
    public MemoryDeductionAllocationStore Inner { get; set; } = inner;
    public DeductionAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionAllocation? Find(string code)
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

    public void Save(DeductionAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction allocations. Empty fields match all.</summary>
public sealed record DeductionAllocationQuery(string Title, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionAllocation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<DeductionAllocation> Apply(List<DeductionAllocation> items)
    {
        var matched = Collections.Filter(items, (DeductionAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction allocation store.</summary>
public static class DeductionAllocationStore
{
    public static IDeductionAllocationStore NewMemoryDeductionAllocationStore()
    {
        return new MemoryDeductionAllocationStore(new Dictionary<string, DeductionAllocation>(), new List<string>());
    }

    public static IDeductionAllocationStore NewCachedDeductionAllocationStore(MemoryDeductionAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionAllocationStore(inner, DeductionAllocationCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DeductionAllocation> FindDeductionAllocations(IDeductionAllocationStore store, DeductionAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
