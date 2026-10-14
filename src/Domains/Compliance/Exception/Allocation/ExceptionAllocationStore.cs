namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps exception allocations by code.</summary>
public interface IExceptionAllocationStore
{
    ExceptionAllocation? Find(string code);
    void Save(ExceptionAllocation item);
    bool Remove(string code);
    List<ExceptionAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryExceptionAllocationStore(Dictionary<string, ExceptionAllocation> items, List<string> order) : IExceptionAllocationStore
{
    public Dictionary<string, ExceptionAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ExceptionAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ExceptionAllocation item)
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

    public List<ExceptionAllocation> List()
    {
        var outValue = new List<ExceptionAllocation>();
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
public sealed class CachedExceptionAllocationStore(MemoryExceptionAllocationStore inner, ExceptionAllocationCache cache, int hits, int misses) : IExceptionAllocationStore
{
    public MemoryExceptionAllocationStore Inner { get; set; } = inner;
    public ExceptionAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ExceptionAllocation? Find(string code)
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

    public void Save(ExceptionAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ExceptionAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects exception allocations. Empty fields match all.</summary>
public sealed record ExceptionAllocationQuery(string Team, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(ExceptionAllocation item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ExceptionAllocation> Apply(List<ExceptionAllocation> items)
    {
        var matched = Collections.Filter(items, (ExceptionAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of exception allocation store.</summary>
public static class ExceptionAllocationStore
{
    public static IExceptionAllocationStore NewMemoryExceptionAllocationStore()
    {
        return new MemoryExceptionAllocationStore(new Dictionary<string, ExceptionAllocation>(), new List<string>());
    }

    public static IExceptionAllocationStore NewCachedExceptionAllocationStore(MemoryExceptionAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedExceptionAllocationStore(inner, ExceptionAllocationCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ExceptionAllocation> FindExceptionAllocations(IExceptionAllocationStore store, ExceptionAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
