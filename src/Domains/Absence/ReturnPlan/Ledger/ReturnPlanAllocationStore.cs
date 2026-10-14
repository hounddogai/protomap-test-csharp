namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan allocations by code.</summary>
public interface IReturnPlanAllocationStore
{
    ReturnPlanAllocation? Find(string code);
    void Save(ReturnPlanAllocation item);
    bool Remove(string code);
    List<ReturnPlanAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanAllocationStore(Dictionary<string, ReturnPlanAllocation> items, List<string> order) : IReturnPlanAllocationStore
{
    public Dictionary<string, ReturnPlanAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanAllocation item)
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

    public List<ReturnPlanAllocation> List()
    {
        var outValue = new List<ReturnPlanAllocation>();
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
public sealed class CachedReturnPlanAllocationStore(MemoryReturnPlanAllocationStore inner, ReturnPlanAllocationCache cache, int hits, int misses) : IReturnPlanAllocationStore
{
    public MemoryReturnPlanAllocationStore Inner { get; set; } = inner;
    public ReturnPlanAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanAllocation? Find(string code)
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

    public void Save(ReturnPlanAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan allocations. Empty fields match all.</summary>
public sealed record ReturnPlanAllocationQuery(string Locale, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanAllocation item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanAllocation> Apply(List<ReturnPlanAllocation> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan allocation store.</summary>
public static class ReturnPlanAllocationStore
{
    public static IReturnPlanAllocationStore NewMemoryReturnPlanAllocationStore()
    {
        return new MemoryReturnPlanAllocationStore(
            new Dictionary<string, ReturnPlanAllocation>(),
            new List<string>());
    }

    public static IReturnPlanAllocationStore NewCachedReturnPlanAllocationStore(MemoryReturnPlanAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanAllocationStore(inner, ReturnPlanAllocationCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanAllocation> FindReturnPlanAllocations(IReturnPlanAllocationStore store, ReturnPlanAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
