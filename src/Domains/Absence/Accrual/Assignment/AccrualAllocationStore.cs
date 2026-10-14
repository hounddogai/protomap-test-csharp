namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual allocations by code.</summary>
public interface IAccrualAllocationStore
{
    AccrualAllocation? Find(string code);
    void Save(AccrualAllocation item);
    bool Remove(string code);
    List<AccrualAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualAllocationStore(Dictionary<string, AccrualAllocation> items, List<string> order) : IAccrualAllocationStore
{
    public Dictionary<string, AccrualAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualAllocation item)
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

    public List<AccrualAllocation> List()
    {
        var outValue = new List<AccrualAllocation>();
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
public sealed class CachedAccrualAllocationStore(MemoryAccrualAllocationStore inner, AccrualAllocationCache cache, int hits, int misses) : IAccrualAllocationStore
{
    public MemoryAccrualAllocationStore Inner { get; set; } = inner;
    public AccrualAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualAllocation? Find(string code)
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

    public void Save(AccrualAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual allocations. Empty fields match all.</summary>
public sealed record AccrualAllocationQuery(string Unit, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualAllocation item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualAllocation> Apply(List<AccrualAllocation> items)
    {
        var matched = Collections.Filter(items, (AccrualAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual allocation store.</summary>
public static class AccrualAllocationStore
{
    public static IAccrualAllocationStore NewMemoryAccrualAllocationStore()
    {
        return new MemoryAccrualAllocationStore(new Dictionary<string, AccrualAllocation>(), new List<string>());
    }

    public static IAccrualAllocationStore NewCachedAccrualAllocationStore(MemoryAccrualAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualAllocationStore(inner, AccrualAllocationCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AccrualAllocation> FindAccrualAllocations(IAccrualAllocationStore store, AccrualAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
