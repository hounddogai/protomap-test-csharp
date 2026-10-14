namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global pay band allocations by code.</summary>
public interface IGlobalPayBandAllocationStore
{
    GlobalPayBandAllocation? Find(string code);
    void Save(GlobalPayBandAllocation item);
    bool Remove(string code);
    List<GlobalPayBandAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPayBandAllocationStore(Dictionary<string, GlobalPayBandAllocation> items, List<string> order) : IGlobalPayBandAllocationStore
{
    public Dictionary<string, GlobalPayBandAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPayBandAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPayBandAllocation item)
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

    public List<GlobalPayBandAllocation> List()
    {
        var outValue = new List<GlobalPayBandAllocation>();
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
public sealed class CachedGlobalPayBandAllocationStore(MemoryGlobalPayBandAllocationStore inner, GlobalPayBandAllocationCache cache, int hits, int misses) : IGlobalPayBandAllocationStore
{
    public MemoryGlobalPayBandAllocationStore Inner { get; set; } = inner;
    public GlobalPayBandAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPayBandAllocation? Find(string code)
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

    public void Save(GlobalPayBandAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPayBandAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global pay band allocations. Empty fields match all.</summary>
public sealed record GlobalPayBandAllocationQuery(string Channel, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPayBandAllocation item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPayBandAllocation> Apply(List<GlobalPayBandAllocation> items)
    {
        var matched = Collections.Filter(items, (GlobalPayBandAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global pay band allocation store.</summary>
public static class GlobalPayBandAllocationStore
{
    public static IGlobalPayBandAllocationStore NewMemoryGlobalPayBandAllocationStore()
    {
        return new MemoryGlobalPayBandAllocationStore(
            new Dictionary<string, GlobalPayBandAllocation>(),
            new List<string>());
    }

    public static IGlobalPayBandAllocationStore NewCachedGlobalPayBandAllocationStore(MemoryGlobalPayBandAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPayBandAllocationStore(
            inner,
            GlobalPayBandAllocationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<GlobalPayBandAllocation> FindGlobalPayBandAllocations(IGlobalPayBandAllocationStore store, GlobalPayBandAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
