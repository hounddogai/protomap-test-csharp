namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay allocations by code.</summary>
public interface IRetroPayAllocationStore
{
    RetroPayAllocation? Find(string code);
    void Save(RetroPayAllocation item);
    bool Remove(string code);
    List<RetroPayAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayAllocationStore(Dictionary<string, RetroPayAllocation> items, List<string> order) : IRetroPayAllocationStore
{
    public Dictionary<string, RetroPayAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayAllocation item)
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

    public List<RetroPayAllocation> List()
    {
        var outValue = new List<RetroPayAllocation>();
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
public sealed class CachedRetroPayAllocationStore(MemoryRetroPayAllocationStore inner, RetroPayAllocationCache cache, int hits, int misses) : IRetroPayAllocationStore
{
    public MemoryRetroPayAllocationStore Inner { get; set; } = inner;
    public RetroPayAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayAllocation? Find(string code)
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

    public void Save(RetroPayAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay allocations. Empty fields match all.</summary>
public sealed record RetroPayAllocationQuery(string Category, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayAllocation item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayAllocation> Apply(List<RetroPayAllocation> items)
    {
        var matched = Collections.Filter(items, (RetroPayAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay allocation store.</summary>
public static class RetroPayAllocationStore
{
    public static IRetroPayAllocationStore NewMemoryRetroPayAllocationStore()
    {
        return new MemoryRetroPayAllocationStore(new Dictionary<string, RetroPayAllocation>(), new List<string>());
    }

    public static IRetroPayAllocationStore NewCachedRetroPayAllocationStore(MemoryRetroPayAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayAllocationStore(inner, RetroPayAllocationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RetroPayAllocation> FindRetroPayAllocations(IRetroPayAllocationStore store, RetroPayAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
